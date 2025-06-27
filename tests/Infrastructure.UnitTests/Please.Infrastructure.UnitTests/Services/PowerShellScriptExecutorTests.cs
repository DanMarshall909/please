using Microsoft.Extensions.Logging;
using NSubstitute;
using Please.Infrastructure.Services;

namespace Please.Infrastructure.UnitTests.Services;

public class PowerShellScriptExecutorTests
{
    private PowerShellScriptExecutor createScriptExecutor()
    {
        var logger = Substitute.For<ILogger<PowerShellScriptExecutor>>();
        return new PowerShellScriptExecutor(logger);
    }

    [Fact]
    public async Task Test_write_host_output_is_captured_successfully()
    {
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
    public async Task Test_mixed_output_types_are_captured()
    {
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
    public async Task Test_empty_script_returns_empty_output()
    {
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
    public async Task Test_script_with_error_returns_failure()
    {
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
    public async Task Test_markdown_code_fences_are_cleaned()
    {
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
