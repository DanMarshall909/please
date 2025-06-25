using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Infrastructure.Services;

namespace Please.Infrastructure.UnitTests.Services;

[TestFixture]
public class PowerShellScriptExecutorTests
{
    private PowerShellScriptExecutor _scriptExecutor;
    private ILogger<PowerShellScriptExecutor> _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<PowerShellScriptExecutor>>();
        _scriptExecutor = new PowerShellScriptExecutor(_logger);
    }

    [Test]
    public async Task Test_write_host_output_is_captured_successfully()
    {
        // Given: A script with Write-Host commands
        var script = """
            Write-Host "Hello World!"
            Write-Host "Line 2"
            """;

        // When: We execute the script
        var result = await _scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should capture the Write-Host output
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Contains.Substring("Hello World!"));
        Assert.That(result.Value, Contains.Substring("Line 2"));
    }

    [Test]
    public async Task Test_mixed_output_types_are_captured()
    {
        // Given: A script with Write-Host and regular output
        var script = """
            Write-Host "From Write-Host"
            "From Write-Output"
            $name = "PowerShell"
            Write-Host "Hello $name!"
            """;

        // When: We execute the script
        var result = await _scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should capture both types of output
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Contains.Substring("From Write-Host"));
        Assert.That(result.Value, Contains.Substring("Hello PowerShell!"));
    }

    [Test]
    public async Task Test_color_script_example_works()
    {
        // Given: The specific color script from the user's example
        var script = """
            # List of 10 different colors
            $colors = @("Red", "Orange", "Yellow", "Green", "Blue", "Purple", "Pink", "Brown", "Black", "White")

            # Loop through each color and say hi
            foreach ($color in $colors) {
                Write-Host "Hi in $color!"
            }
            """;

        // When: We execute the script
        var result = await _scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should capture all the Write-Host output
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Contains.Substring("Hi in Red!"));
        Assert.That(result.Value, Contains.Substring("Hi in Blue!"));
        Assert.That(result.Value, Contains.Substring("Hi in White!"));

        // Verify we get all 10 color messages
        var lines = result.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var colorLines = lines.Where(line => line.Contains("Hi in") && line.Contains("!")).ToList();
        Assert.That(colorLines.Count, Is.EqualTo(10));
    }

    [Test]
    public async Task Test_empty_script_returns_empty_output()
    {
        // Given: An empty script
        var script = "";

        // When: We execute the script
        var result = await _scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should succeed with empty output
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(string.IsNullOrWhiteSpace(result.Value), Is.True);
    }

    [Test]
    public async Task Test_script_with_error_returns_failure()
    {
        // Given: A script that will cause an error
        var script = "Get-NonExistentCommand";

        // When: We execute the script
        var result = await _scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should return failure
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task Test_markdown_code_fences_are_cleaned()
    {
        // Given: A script with markdown code fences
        var script = """
            ```powershell
            Write-Host "Hello from PowerShell!"
            ```
            """;

        // When: We execute the script
        var result = await _scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should execute successfully and capture output
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Contains.Substring("Hello from PowerShell!"));
    }
}
