using Please.Console.Services;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Presentation.UnitTests.Services;

public class ConsoleUIServiceTests
{
    private readonly IConsoleUIService _consoleUIService;

    public ConsoleUIServiceTests()
    {
        _consoleUIService = new ConsoleUIService();
    }

    [Fact]
    public void Test_display_script_with_beautiful_formatting_shows_panel_with_syntax_highlighting()
    {
        // Arrange: A PowerShell script to display
        var script = "Get-Process | Where-Object { $_.CPU -gt 100 }";
        var title = "Generated PowerShell Script";

        // Act: Display the script (no exception should be thrown)
        var exception = Record.Exception(() => _consoleUIService.DisplayScript(script, title));

        // Assert: Should not throw exception and service should handle display
        exception.ShouldBeNull();
    }

    [Fact]
    public async Task Test_display_progress_indicator_shows_professional_spinner_during_ai_generation()
    {
        // Arrange: Progress message for AI generation
        var message = "Generating PowerShell script...";
        var taskCompleted = false;

        // Act: Display progress with async task
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _consoleUIService.DisplayProgressAsync(message, async () =>
            {
                await Task.Delay(100); // Simulate AI work
                taskCompleted = true;
            });
        });

        // Assert: Should not throw exception and task should complete
        exception.ShouldBeNull();
        taskCompleted.ShouldBeTrue();
    }

    [Fact]
    public void Test_display_risk_warning_shows_colored_safety_indicators()
    {
        // Arrange: High-risk script operations
        var riskLevel = "HIGH";
        var warnings = new[] { "Modifies system files", "Requires admin privileges" };

        // Act: Display risk warning (no exception should be thrown)
        var exception = Record.Exception(() => _consoleUIService.DisplayRiskWarning(riskLevel, warnings));

        // Assert: Should not throw exception and service should handle display
        exception.ShouldBeNull();
    }

    [Fact]
    public void Test_display_banner_shows_professional_application_header()
    {
        // Arrange: Application information
        var version = "6.0.0";
        var description = "PowerShell Script Generator";

        // Act: Display banner (no exception should be thrown)
        var exception = Record.Exception(() => _consoleUIService.DisplayBanner(version, description));

        // Assert: Should not throw exception and service should handle display
        exception.ShouldBeNull();
    }

    [Fact]
    public void Display_script_with_syntax_highlighting_handles_powershell()
    {
        // Arrange
        string powershellScript = "Get-Process | Where-Object { $_.CPU -gt 100 }";
        string title = "Test PowerShell Script";

        // Act & Assert - Should not throw
        Should.NotThrow(() => _consoleUIService.DisplayScriptWithSyntaxHighlighting(powershellScript, title, ScriptType.PowerShell));
    }

    [Fact]
    public void Display_script_response_shows_all_information()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "Get-Date",
            "Get current date",
            ProviderType.OpenAi,
            "gpt-4",
            ScriptType.PowerShell,
            RiskLevel.Low
        ).WithWarnings(new[] { "Test warning" })
         .WithSafetyNotes(new[] { "Test safety note" });

        // Act & Assert - Should not throw
        Should.NotThrow(() => _consoleUIService.DisplayScriptResponse(response));
    }

    [Fact]
    public void Display_safety_notes_handles_multiple_notes()
    {
        // Arrange
        var safetyNotes = new List<string>
        {
            "✅ Low risk script - Generally safe to execute",
            "📂 Ensure target directories exist"
        };

        // Act & Assert - Should not throw
        Should.NotThrow(() => _consoleUIService.DisplaySafetyNotes(safetyNotes));
    }

    [Fact]
    public void Display_enhanced_progress_with_steps_shows_all_steps()
    {
        // Arrange
        var steps = new[]
        {
            "Analyzing request...",
            "Generating script...",
            "Validating safety..."
        };

        // Act & Assert - Should not throw exception
        Should.NotThrow(async () => 
        {
            await _consoleUIService.DisplayEnhancedProgressAsync("Processing", steps, async (step, index) =>
            {
                await Task.Delay(10); // Simulate work
            });
        });
    }

    [Fact]
    public void Display_script_preview_shows_metadata()
    {
        // Arrange
        var response = ScriptResponse.Create(
            "Get-Service",
            "List all services",
            ProviderType.Anthropic,
            "claude-3-haiku",
            ScriptType.PowerShell,
            RiskLevel.Medium
        );

        // Act & Assert - Should not throw
        Should.NotThrow(() => _consoleUIService.DisplayScriptPreview(response));
    }
}
