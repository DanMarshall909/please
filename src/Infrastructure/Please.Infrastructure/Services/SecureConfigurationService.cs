using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.ValueObjects;
#if WINDOWS
using System.Security.Cryptography;
#endif

namespace Please.Infrastructure.Services;

/// <summary>
/// Secure configuration service that manages API keys with encryption and validation.
/// Priority chain: Environment Variables → Encrypted Storage → User Secrets → Interactive Prompt
/// </summary>
public class SecureConfigurationService : ISecureConfigurationService
{
    private readonly ILogger<SecureConfigurationService> _logger;
    private readonly ISecureInputService _secureInputService;
    private readonly Dictionary<ProviderType, SecureString> _memoryCache;
    private readonly string _configDirectory;
    private readonly string _encryptedConfigPath;

    public SecureConfigurationService(
        ILogger<SecureConfigurationService> logger,
        ISecureInputService secureInputService)
    {
        _logger = logger;
        _secureInputService = secureInputService;
        _memoryCache = new Dictionary<ProviderType, SecureString>();

        // Store encrypted config in user's AppData directory
        _configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Please");
        _encryptedConfigPath = Path.Combine(_configDirectory, "config.encrypted");

        ensureConfigDirectoryExists();
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
                return cachedKey.ToUnsecureString();
            }

            // Step 2: Check environment variables
            var envKey = getEnvironmentVariableKey(provider);
            if (!string.IsNullOrEmpty(envKey))
            {
                _logger.LogDebug("Found API key in environment variables for {Provider}", provider);
                var secureKey = SecureString.Create(envKey);
                _memoryCache[provider] = secureKey;
                return envKey;
            }

            // Step 3: Check encrypted local storage
            var storedKey = await getStoredApiKeyAsync(provider);
            if (!string.IsNullOrEmpty(storedKey))
            {
                _logger.LogDebug("Found API key in encrypted storage for {Provider}", provider);
                var secureKey = SecureString.Create(storedKey);
                _memoryCache[provider] = secureKey;
                return storedKey;
            }

            // Step 4: Check user secrets (development environment)
            var userSecretKey = getUserSecretKey(provider);
            if (!string.IsNullOrEmpty(userSecretKey))
            {
                _logger.LogDebug("Found API key in user secrets for {Provider}", provider);
                var secureKey = SecureString.Create(userSecretKey);
                _memoryCache[provider] = secureKey;
                return userSecretKey;
            }

            // Step 5: Interactive prompt as last resort
            _logger.LogInformation("No API key found for {Provider}, prompting user", provider);
            var promptedKey = await promptForApiKeyAsync(provider);
            if (!string.IsNullOrEmpty(promptedKey))
            {
                // Store the key for future use
                await StoreApiKeyAsync(provider, promptedKey);
                var secureKey = SecureString.Create(promptedKey);
                _memoryCache[provider] = secureKey;
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
        try
        {
            _logger.LogDebug("Storing API key for provider: {Provider}", provider);

            // Validate the API key first
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            }

            if (!_secureInputService.ValidateSecureInput(apiKey))
            {
                throw new ArgumentException("API key contains invalid characters", nameof(apiKey));
            }

            // Load existing config or create new
            var config = await loadEncryptedConfigAsync() ?? new Dictionary<string, string>();

            // Encrypt and store the API key
            var encryptedKey = encryptString(apiKey);
            config[provider.ToString()] = encryptedKey;

            // Save the updated config
            await saveEncryptedConfigAsync(config);

            // Update memory cache
            var secureKey = SecureString.Create(apiKey);
            _memoryCache[provider] = secureKey;

            _logger.LogInformation("API key stored successfully for {Provider}", provider);
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

            // Basic format validation
            if (apiKey.Length < 20 || apiKey.Length > 200)
            {
                _logger.LogWarning("API key for {Provider} has invalid length: {Length}", provider, apiKey.Length);
                return false;
            }

            // For now, we'll just do basic validation
            // In a full implementation, you'd make a minimal API call to validate
            _logger.LogDebug("API key for {Provider} passed basic validation", provider);
            return true;
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

            // Dispose all cached secure strings
            foreach (var kvp in _memoryCache)
            {
                kvp.Value?.Dispose();
            }

            _memoryCache.Clear();

            // Force garbage collection to clear any remaining sensitive data
            GC.Collect();
            GC.WaitForPendingFinalizers();
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

    private string getUserSecretKey(ProviderType provider)
    {
        // In a real implementation, this would read from user secrets
        // For now, we'll just check a simple pattern
        var secretName = $"Please:{provider}:ApiKey";
        return Environment.GetEnvironmentVariable(secretName) ?? string.Empty;
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
            var config = await loadEncryptedConfigAsync();
            if (config?.TryGetValue(provider.ToString(), out var encryptedKey) == true)
            {
                return decryptString(encryptedKey);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading stored API key for {Provider}", provider);
            return null;
        }
    }

    private async Task<Dictionary<string, string>?> loadEncryptedConfigAsync()
    {
        try
        {
            if (!File.Exists(_encryptedConfigPath))
            {
                return null;
            }

            var encryptedContent = await File.ReadAllTextAsync(_encryptedConfigPath);
            var jsonContent = decryptString(encryptedContent);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading encrypted configuration");
            return null;
        }
    }

    private async Task saveEncryptedConfigAsync(Dictionary<string, string> config)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var encryptedContent = encryptString(jsonContent);

            await File.WriteAllTextAsync(_encryptedConfigPath, encryptedContent);

            // Set file permissions to be more restrictive (Windows)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var fileInfo = new FileInfo(_encryptedConfigPath);
                var fileSecurity = fileInfo.GetAccessControl();
                // In a full implementation, you'd set more restrictive permissions here
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving encrypted configuration");
            throw;
        }
    }

    private string encryptString(string plainText)
    {
        // For now, use simple AES encryption for all platforms
        // In production, you'd use Windows DPAPI on Windows
        var key = getMachineSpecificKey();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

        // Combine IV and encrypted data
        var combined = new byte[aes.IV.Length + encryptedBytes.Length];
        Array.Copy(aes.IV, 0, combined, 0, aes.IV.Length);
        Array.Copy(encryptedBytes, 0, combined, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(combined);
    }

    private string decryptString(string encryptedText)
    {
        // For now, use simple AES decryption for all platforms
        // In production, you'd use Windows DPAPI on Windows
        var key = getMachineSpecificKey();
        var combined = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.Key = key;

        // Extract IV and encrypted data
        var iv = new byte[16]; // AES block size
        var encryptedBytes = new byte[combined.Length - 16];
        Array.Copy(combined, 0, iv, 0, 16);
        Array.Copy(combined, 16, encryptedBytes, 0, encryptedBytes.Length);

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    private byte[] getMachineSpecificKey()
    {
        // Generate a machine-specific key based on hardware characteristics
        // This is a simplified approach - in production you'd want more robust key derivation
        var machineInfo = Environment.MachineName + Environment.UserName + Environment.OSVersion.ToString();
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
    }

    private void ensureConfigDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
                _logger.LogDebug("Created configuration directory: {Directory}", _configDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration directory: {Directory}", _configDirectory);
            throw;
        }
    }
}
