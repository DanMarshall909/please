using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Services;

/// <summary>
/// Secure configuration service using .NET Data Protection API for cross-platform encryption.
/// Priority chain: Environment Variables → Encrypted Storage → User Secrets → Interactive Prompt
/// </summary>
public class SecureConfigurationService : ISecureConfigurationService, IDisposable
{
    private readonly ILogger<SecureConfigurationService> _logger;
    private readonly IDataProtector _dataProtector;
    private readonly IConfiguration _configuration;
    private readonly ISecureInputService _secureInputService;
    private readonly string _storageDirectory;
    private readonly Dictionary<ProviderType, string> _memoryCache;
    private readonly SemaphoreSlim _fileLock;
    private bool _disposed;

    public SecureConfigurationService(
        ILogger<SecureConfigurationService> logger,
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration,
        ISecureInputService secureInputService,
        string? storageDirectory = null)
    {
        _logger = logger;
        _dataProtector = dataProtectionProvider.CreateProtector("Please.ApiKeys");
        _configuration = configuration;
        _secureInputService = secureInputService;

        _storageDirectory = storageDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".please", "keys");

        _memoryCache = new Dictionary<ProviderType, string>();
        _fileLock = new SemaphoreSlim(1, 1);

        ensureStorageDirectoryExists();
    }

    public async Task<string?> GetApiKeyAsync(ProviderType provider)
    {
        try
        {
            _logger.LogDebug("Retrieving API key for provider: {Provider}", provider);

            // Step 1: Check memory cache first
            if (_memoryCache.TryGetValue(provider, out var cachedKey))
            {
                _logger.LogDebug("Found API key in memory cache for {Provider}", provider);
                return cachedKey;
            }

            // Step 2: Check environment variables (highest priority)
            var envKey = getEnvironmentVariableKey(provider);
            if (!string.IsNullOrEmpty(envKey))
            {
                _logger.LogDebug("Found API key in environment variables for {Provider}", provider);
                _memoryCache[provider] = envKey;
                return envKey;
            }

            // Step 3: Check encrypted local storage
            var storedKey = await getStoredApiKeyAsync(provider);
            if (!string.IsNullOrEmpty(storedKey))
            {
                _logger.LogDebug("Found API key in encrypted storage for {Provider}", provider);
                _memoryCache[provider] = storedKey;
                return storedKey;
            }

            // Step 4: Check configuration (User Secrets, appsettings.json)
            var configKey = getConfigurationKey(provider);
            if (!string.IsNullOrEmpty(configKey))
            {
                _logger.LogDebug("Found API key in configuration for {Provider}", provider);
                _memoryCache[provider] = configKey;
                return configKey;
            }

            // Step 5: Interactive prompt as last resort
            _logger.LogInformation("No API key found for {Provider}, prompting user", provider);
            var promptedKey = await promptForApiKeyAsync(provider);
            if (!string.IsNullOrEmpty(promptedKey))
            {
                // Store the key for future use
                await StoreApiKeyAsync(provider, promptedKey);
                _memoryCache[provider] = promptedKey;
                return promptedKey;
            }

            _logger.LogWarning("No API key could be obtained for {Provider}", provider);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API key for {Provider}", provider);
            return null;
        }
    }

    public async Task StoreApiKeyAsync(ProviderType provider, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
        }

        try
        {
            _logger.LogDebug("Storing API key for provider: {Provider}", provider);

            await _fileLock.WaitAsync();
            try
            {
                var filePath = getKeyFilePath(provider);
                var keyBytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
                var encryptedBytes = _dataProtector.Protect(keyBytes);
                var encryptedBase64 = Convert.ToBase64String(encryptedBytes);

                await File.WriteAllTextAsync(filePath, encryptedBase64);

                // Update memory cache
                _memoryCache[provider] = apiKey;

                _logger.LogInformation("API key stored successfully for {Provider}", provider);
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing API key for {Provider}", provider);
            throw;
        }
    }

    public async Task<bool> ValidateApiKeyAsync(ProviderType provider)
    {
        try
        {
            var apiKey = await GetApiKeyAsync(provider);
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            // Basic format validation - in production, you'd make actual API calls
            return apiKey.Length >= 20 && apiKey.Length <= 200;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key for {Provider}", provider);
            return false;
        }
    }

    public async Task<bool> HasValidApiKeyAsync(ProviderType provider)
    {
        try
        {
            var apiKey = await GetApiKeyAsync(provider);
            return !string.IsNullOrEmpty(apiKey) && await ValidateApiKeyAsync(provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if valid API key exists for {Provider}", provider);
            return false;
        }
    }

    public void ClearSensitiveData()
    {
        try
        {
            _logger.LogDebug("Clearing sensitive data from memory");
            _memoryCache.Clear();
            GC.Collect();
            _logger.LogDebug("Sensitive data cleared from memory");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing sensitive data");
        }
    }

    private string getEnvironmentVariableKey(ProviderType provider)
    {
        var envVarName = $"PLEASE_{provider.ToString().ToUpperInvariant()}_API_KEY";
        return Environment.GetEnvironmentVariable(envVarName) ?? string.Empty;
    }

    private string getConfigurationKey(ProviderType provider)
    {
        return _configuration[$"Providers:{provider}:ApiKey"] ?? string.Empty;
    }

    private async Task<string> promptForApiKeyAsync(ProviderType provider)
    {
        var prompt = $"Please enter your {provider} API key: ";
        return await _secureInputService.PromptForSecureInputAsync(prompt);
    }

    private async Task<string?> getStoredApiKeyAsync(ProviderType provider)
    {
        try
        {
            var filePath = getKeyFilePath(provider);
            if (!File.Exists(filePath))
            {
                return null;
            }

            await _fileLock.WaitAsync();
            try
            {
                var encryptedBase64 = await File.ReadAllTextAsync(filePath);
                var encryptedBytes = Convert.FromBase64String(encryptedBase64);
                var decryptedBytes = _dataProtector.Unprotect(encryptedBytes);
                return System.Text.Encoding.UTF8.GetString(decryptedBytes);
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading stored API key for {Provider}", provider);
            return null;
        }
    }

    private string getKeyFilePath(ProviderType provider)
    {
        return Path.Combine(_storageDirectory, $"{provider.ToString().ToLowerInvariant()}.key");
    }

    private void ensureStorageDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
                _logger.LogDebug("Created storage directory: {Directory}", _storageDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating storage directory: {Directory}", _storageDirectory);
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ClearSensitiveData();
            _fileLock?.Dispose();
            _disposed = true;
        }
    }
}
