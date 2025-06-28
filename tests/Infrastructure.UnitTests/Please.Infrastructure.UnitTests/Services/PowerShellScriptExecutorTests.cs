using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Infrastructure.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Please.Infrastructure.UnitTests.Services;

public class PowerShellScriptExecutorTests : IDisposable
{
    private readonly bool _isPowerShellAvailable;

    public PowerShellScriptExecutorTests()
    {
        _isPowerShellAvailable = CheckPowerShellAvailability();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    private static bool CheckPowerShellAvailability()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh",
                Arguments = OperatingSystem.IsWindows() ? "-Version" : "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            if (process == null) return false;
            process.WaitForExit(1000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    private PowerShellScriptExecutor createScriptExecutor()
    {
        var logger = Substitute.For<ILogger<PowerShellScriptExecutor>>();
        return new PowerShellScriptExecutor(logger);
    }

    [Fact]
    public async Task Captures_write_host_output_when_script_executed()
    {
        if (!_isPowerShellAvailable)
        {
            // Skip test if PowerShell is not available
            return;
        }
        // Given: A script with Write-Host commands
        var script = """
            Write-Host "Hello World!"
            Write-Host "Line 2"
            """;
        var scriptExecutor = createScriptExecutor();

        // When: We execute the script
        var result = await scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should capture the Write-Host output
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldContain("Hello World!");
        result.Value.ShouldContain("Line 2");
    }

    [Fact]
    public async Task Captures_both_write_host_and_write_output_when_script_has_mixed_commands()
    {
        if (!_isPowerShellAvailable)
        {
            // Skip test if PowerShell is not available
            return;
        }
        // Given: A script with Write-Host and regular output
        var script = """
            Write-Host "From Write-Host"
            "From Write-Output"
            $name = "PowerShell"
            Write-Host "Hello $name!"
            """;
        var scriptExecutor = createScriptExecutor();

        // When: We execute the script
        var result = await scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should capture both types of output
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldContain("From Write-Host");
        result.Value.ShouldContain("Hello PowerShell!");
    }

    [Fact]
    public async Task Executes_color_script_successfully_and_captures_all_color_messages()
    {
        if (!_isPowerShellAvailable)
        {
            // Skip test if PowerShell is not available
            return;
        }
        // Given: The specific color script from the user's example
        var script = """
            # List of 10 different colors
            $colors = @("Red", "Orange", "Yellow", "Green", "Blue", "Purple", "Pink", "Brown", "Black", "White")

            # Loop through each color and say hi
            foreach ($color in $colors) {
                Write-Host "Hi in $color!"
            }
            """;
        var scriptExecutor = createScriptExecutor();

        // When: We execute the script
        var result = await scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should capture all the Write-Host output
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldContain("Hi in Red!");
        result.Value.ShouldContain("Hi in Blue!");
        result.Value.ShouldContain("Hi in White!");

        // Verify we get all 10 color messages
        var lines = result.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var colorLines = lines.Where(line => line.Contains("Hi in") && line.Contains("!")).ToList();
        colorLines.Count.ShouldBe(10);
    }

    [Fact]
    public async Task Returns_empty_output_when_script_is_empty()
    {
        if (!_isPowerShellAvailable)
        {
            // Skip test if PowerShell is not available
            return;
        }
        // Given: An empty script
        var script = "";
        var scriptExecutor = createScriptExecutor();

        // When: We execute the script
        var result = await scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should succeed with empty output
        result.IsSuccess.ShouldBeTrue();
        string.IsNullOrWhiteSpace(result.Value).ShouldBeTrue();
    }

    [Fact]
    public async Task Returns_failure_when_script_contains_syntax_errors()
    {
        if (!_isPowerShellAvailable)
        {
            // Skip test if PowerShell is not available
            return;
        }
        // Given: A script that will cause an error
        var script = "Get-NonExistentCommand";
        var scriptExecutor = createScriptExecutor();

        // When: We execute the script
        var result = await scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should return failure
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Removes_markdown_code_fences_before_executing_script()
    {
        if (!_isPowerShellAvailable)
        {
            // Skip test if PowerShell is not available
            return;
        }
        // Given: A script with markdown code fences
        var script = """
            ```powershell
            Write-Host "Hello from PowerShell!"
            ```
            """;
        var scriptExecutor = createScriptExecutor();

        // When: We execute the script
        var result = await scriptExecutor.ExecuteScriptAsync(script);

        // Then: Should execute successfully and capture output
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldContain("Hello from PowerShell!");
    }
}
