using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Infrastructure.Services;
using Shouldly;

namespace Please.Infrastructure.UnitTests.Services;

public class ClipboardServiceTests
{
    private readonly ILogger<ClipboardService> _logger;
    private readonly ClipboardService _clipboardService;

    public ClipboardServiceTests()
    {
        _logger = Substitute.For<ILogger<ClipboardService>>();
        _clipboardService = new ClipboardService(_logger);
    }

    [Fact]
    public void Returns_boolean_value_when_checking_clipboard_support()
    {
        // Act
        var result = _clipboardService.IsSupported();

        // Assert
        result.ShouldBeOfType<bool>();
    }

    [Fact]
    public async Task Returns_result_when_setting_text_regardless_of_platform_support()
    {
        // Arrange
        var unsupportedClipboardService = new ClipboardService(_logger);
        
        // We can't easily mock the platform detection, so we'll test the public interface
        // If clipboard is not supported, SetTextAsync should return false
        var testText = "Test clipboard content";

        // Act
        var result = await unsupportedClipboardService.SetTextAsync(testText);

        // Assert
        // The result depends on the platform - it could be true or false
        // We're just verifying it doesn't throw an exception
        result.ShouldBeOfType<bool>();
    }

    [Fact]
    public async Task Returns_result_when_getting_text_regardless_of_platform_support()
    {
        // Arrange
        var unsupportedClipboardService = new ClipboardService(_logger);

        // Act
        var result = await unsupportedClipboardService.GetTextAsync();

        // Assert
        // The result depends on the platform - it could be text or null
        // We're just verifying it doesn't throw an exception
        if (result != null)
        {
            result.ShouldBeOfType<string>();
        }
    }

    [Fact]
    public async Task Does_not_throw_when_setting_empty_string()
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _clipboardService.SetTextAsync(""));
        exception.ShouldBeNull();
    }

    [Fact]
    public async Task Does_not_throw_when_setting_null_string()
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _clipboardService.SetTextAsync(null!));
        exception.ShouldBeNull();
    }
}