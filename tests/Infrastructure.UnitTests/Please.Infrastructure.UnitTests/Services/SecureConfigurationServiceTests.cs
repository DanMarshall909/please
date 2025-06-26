using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Infrastructure.Services;

namespace Please.Infrastructure.UnitTests.Services;

public class SecureConfigurationServiceTests : IDisposable
{
    private readonly ILogger<SecureConfigurationService> _logger;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IDataProtector _dataProtector;
    private readonly IConfiguration _configuration;
    private readonly ISecureInputService _secureInputService;
    private readonly SecureConfigurationService _service;
    private readonly string _testDirectory;

    public SecureConfigurationServiceTests()
    {
        _logger = Substitute.For<ILogger<SecureConfigurationService>>();
        _dataProtectionProvider = Substitute.For<IDataProtectionProvider>();
        _dataProtector = Substitute.For<IDataProtector>();
        _configuration = Substitute.For<IConfiguration>();
        _secureInputService = Substitute.For<ISecureInputService>();

        _dataProtectionProvider.CreateProtector("Please.ApiKeys").Returns(_dataProtector);

        // Create a temporary directory for testing
        _testDirectory = Path.Combine(Path.GetTempPath(), "PleaseTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        _service = new SecureConfigurationService(
            _logger,
            _dataProtectionProvider,
            _configuration,
            _secureInputService,
            _testDirectory);
    }

    [Fact]
    public async Task GetApiKey_environment_variable_takes_highest_priority()
    {
        // Arrange
        const string expectedKey = "env-test-key";
        Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", expectedKey);

        try
        {
            // Act
            var result = await _service.GetApiKeyAsync(ProviderType.OpenAi);

            // Assert
            Assert.Equal(expectedKey, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", null);
        }
    }

    [Fact]
    public async Task GetApiKey_falls_back_to_encrypted_storage_when_env_empty()
    {
        // Arrange
        const string expectedKey = "stored-test-key";
        var encryptedBytes = System.Text.Encoding.UTF8.GetBytes("encrypted-stored-test-key");

        _dataProtector.Protect(Arg.Any<byte[]>()).Returns(encryptedBytes);
        _dataProtector.Unprotect(Arg.Any<byte[]>()).Returns(System.Text.Encoding.UTF8.GetBytes(expectedKey));

        // Store a key first
        await _service.StoreApiKeyAsync(ProviderType.OpenAi, expectedKey);

        // Act
        var result = await _service.GetApiKeyAsync(ProviderType.OpenAi);

        // Assert
        Assert.Equal(expectedKey, result);
    }

    [Fact]
    public async Task GetApiKey_falls_back_to_configuration_when_storage_empty()
    {
        // Arrange
        const string expectedKey = "config-test-key";
        _configuration["Providers:OpenAi:ApiKey"].Returns(expectedKey);

        // Act
        var result = await _service.GetApiKeyAsync(ProviderType.OpenAi);

        // Assert
        Assert.Equal(expectedKey, result);
    }

    [Fact]
    public async Task GetApiKey_prompts_interactively_when_all_sources_empty()
    {
        // Arrange
        const string expectedKey = "prompted-test-key";
        _secureInputService.PromptForSecureInputAsync(Arg.Any<string>()).Returns(expectedKey);

        // Act
        var result = await _service.GetApiKeyAsync(ProviderType.OpenAi);

        // Assert
        Assert.Equal(expectedKey, result);
        await _secureInputService.Received(1).PromptForSecureInputAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetApiKey_returns_null_when_no_interactive_and_no_keys()
    {
        // Arrange
        _secureInputService.PromptForSecureInputAsync(Arg.Any<string>()).Returns(string.Empty);

        // Act
        var result = await _service.GetApiKeyAsync(ProviderType.OpenAi);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task StoreApiKey_encrypts_and_stores_key_properly()
    {
        // Arrange
        const string testKey = "test-api-key";
        var encryptedBytes = System.Text.Encoding.UTF8.GetBytes("encrypted-test-api-key");

        _dataProtector.Protect(Arg.Any<byte[]>()).Returns(encryptedBytes);
        _dataProtector.Unprotect(Arg.Any<byte[]>()).Returns(args =>
        {
            // Return the original key bytes when unprotecting
            return System.Text.Encoding.UTF8.GetBytes(testKey);
        });

        // Act
        await _service.StoreApiKeyAsync(ProviderType.OpenAi, testKey);

        // Assert
        _dataProtector.Received(1).Protect(Arg.Any<byte[]>());

        // Verify key can be retrieved
        var retrievedKey = await _service.GetApiKeyAsync(ProviderType.OpenAi);
        Assert.Equal(testKey, retrievedKey);
    }

    [Fact]
    public async Task StoreApiKey_throws_when_key_is_null_or_empty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.StoreApiKeyAsync(ProviderType.OpenAi, null!));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.StoreApiKeyAsync(ProviderType.OpenAi, string.Empty));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.StoreApiKeyAsync(ProviderType.OpenAi, "   "));
    }

    [Fact]
    public async Task ValidateApiKey_returns_true_for_valid_key_format()
    {
        // Arrange
        const string validKey = "sk-1234567890abcdef1234567890abcdef1234567890abcdef";
        Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", validKey);

        try
        {
            // Act
            var isValid = await _service.ValidateApiKeyAsync(ProviderType.OpenAi);

            // Assert
            Assert.True(isValid);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", null);
        }
    }

    [Fact]
    public async Task ValidateApiKey_returns_false_for_invalid_key_format()
    {
        // Arrange
        const string invalidKey = "short";
        Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", invalidKey);

        try
        {
            // Act
            var isValid = await _service.ValidateApiKeyAsync(ProviderType.OpenAi);

            // Assert
            Assert.False(isValid);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", null);
        }
    }

    [Fact]
    public async Task ValidateApiKey_returns_false_when_no_key_exists()
    {
        // Act
        var isValid = await _service.ValidateApiKeyAsync(ProviderType.OpenAi);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task HasValidApiKey_returns_true_when_key_exists_and_valid()
    {
        // Arrange
        const string validKey = "sk-1234567890abcdef1234567890abcdef1234567890abcdef";
        Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", validKey);

        try
        {
            // Act
            var hasValidKey = await _service.HasValidApiKeyAsync(ProviderType.OpenAi);

            // Assert
            Assert.True(hasValidKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PLEASE_OPENAI_API_KEY", null);
        }
    }

    [Fact]
    public async Task HasValidApiKey_returns_false_when_no_key_exists()
    {
        // Act
        var hasValidKey = await _service.HasValidApiKeyAsync(ProviderType.OpenAi);

        // Assert
        Assert.False(hasValidKey);
    }

    [Fact]
    public void ClearSensitiveData_completes_without_error()
    {
        // Act & Assert (should not throw)
        _service.ClearSensitiveData();
    }

    [Fact]
    public async Task Multiple_concurrent_calls_dont_cause_race_conditions()
    {
        // Arrange
        const string testKey = "concurrent-test-key";
        var encryptedBytes = System.Text.Encoding.UTF8.GetBytes("encrypted-concurrent-test-key");

        _dataProtector.Protect(Arg.Any<byte[]>()).Returns(encryptedBytes);
        _dataProtector.Unprotect(Arg.Any<byte[]>()).Returns(System.Text.Encoding.UTF8.GetBytes(testKey));

        await _service.StoreApiKeyAsync(ProviderType.OpenAi, testKey);

        // Act - Multiple concurrent calls
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _service.GetApiKeyAsync(ProviderType.OpenAi))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.Equal(testKey, result));
    }

    [Fact]
    public async Task StoreApiKey_overwrites_existing_key()
    {
        // Arrange
        const string firstKey = "first-key";
        const string secondKey = "second-key";
        var encryptedFirstBytes = System.Text.Encoding.UTF8.GetBytes("encrypted-first");
        var encryptedSecondBytes = System.Text.Encoding.UTF8.GetBytes("encrypted-second");

        // Setup mock to return different responses based on call order
        var callCount = 0;
        _dataProtector.Protect(Arg.Any<byte[]>()).Returns(args =>
        {
            callCount++;
            return callCount == 1 ? encryptedFirstBytes : encryptedSecondBytes;
        });

        _dataProtector.Unprotect(Arg.Any<byte[]>()).Returns(args =>
        {
            var input = args[0] as byte[];
            if (input?.SequenceEqual(encryptedFirstBytes) == true)
                return System.Text.Encoding.UTF8.GetBytes(firstKey);
            return System.Text.Encoding.UTF8.GetBytes(secondKey);
        });

        // Act
        await _service.StoreApiKeyAsync(ProviderType.OpenAi, firstKey);
        await _service.StoreApiKeyAsync(ProviderType.OpenAi, secondKey);

        // Assert
        var retrievedKey = await _service.GetApiKeyAsync(ProviderType.OpenAi);
        Assert.Equal(secondKey, retrievedKey);
    }

    [Fact]
    public async Task Different_providers_store_keys_independently()
    {
        // Arrange
        const string openAiKey = "openai-key";
        const string anthropicKey = "anthropic-key";
        var encryptedOpenAiBytes = System.Text.Encoding.UTF8.GetBytes("encrypted-openai");
        var encryptedAnthropicBytes = System.Text.Encoding.UTF8.GetBytes("encrypted-anthropic");

        // Track which provider is being called
        var providerCallMap = new Dictionary<string, string>();

        _dataProtector.Protect(Arg.Any<byte[]>()).Returns(args =>
        {
            var input = System.Text.Encoding.UTF8.GetString(args[0] as byte[] ?? Array.Empty<byte>());
            if (input == openAiKey)
                return encryptedOpenAiBytes;
            return encryptedAnthropicBytes;
        });

        _dataProtector.Unprotect(Arg.Any<byte[]>()).Returns(args =>
        {
            var input = args[0] as byte[];
            if (input?.SequenceEqual(encryptedOpenAiBytes) == true)
                return System.Text.Encoding.UTF8.GetBytes(openAiKey);
            return System.Text.Encoding.UTF8.GetBytes(anthropicKey);
        });

        // Act
        await _service.StoreApiKeyAsync(ProviderType.OpenAi, openAiKey);
        await _service.StoreApiKeyAsync(ProviderType.Anthropic, anthropicKey);

        // Assert
        var retrievedOpenAi = await _service.GetApiKeyAsync(ProviderType.OpenAi);
        var retrievedAnthropic = await _service.GetApiKeyAsync(ProviderType.Anthropic);

        Assert.Equal(openAiKey, retrievedOpenAi);
        Assert.Equal(anthropicKey, retrievedAnthropic);
    }

    public void Dispose()
    {
        _service?.Dispose();

        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
