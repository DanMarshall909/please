using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Services;

/// <summary>
/// Executes scripts using PowerShell on Windows platform.
/// </summary>
public class PowerShellScriptExecutor : IScriptExecutor
{
    private readonly ILogger<PowerShellScriptExecutor> _logger;

    public PowerShellScriptExecutor(ILogger<PowerShellScriptExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<Result<string>> ExecuteScriptAsync(string script)
    {
        try
        {
            _logger.LogInformation("Executing PowerShell script...");

            // Clean the script by removing markdown code fences
            var cleanedScript = CleanScript(script);
            _logger.LogInformation("Original script: {OriginalScript}", script);
            _logger.LogInformation("Cleaned script: {CleanedScript}", cleanedScript);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-ExecutionPolicy Bypass -NoProfile -NonInteractive -Command -",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            process.Start();

            // Wrap the script to capture Write-Host output
            var wrappedScript = $@"
$ErrorActionPreference = 'Continue'
$OriginalInformationPreference = $InformationPreference
$InformationPreference = 'Continue'

# Capture all output including Write-Host
$OutputCollection = @()

# Override Write-Host to capture output
function Write-Host {{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [object]$Object,
        [ConsoleColor]$ForegroundColor,
        [ConsoleColor]$BackgroundColor,
        [switch]$NoNewline
    )

    if ($Object -ne $null) {{
        $OutputCollection += $Object.ToString()
        if (-not $NoNewline) {{
            $OutputCollection += ""`n""
        }}
    }}
}}

# Execute the original script
{cleanedScript}

# Output all captured content
$OutputCollection -join """"
";

            // Send the wrapped script to PowerShell
            await process.StandardInput.WriteAsync(wrappedScript);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close();

            // Read output and error streams directly
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = (await outputTask).Trim();
            var error = (await errorTask).Trim();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Script executed successfully with exit code {ExitCode}", process.ExitCode);
                // Return actual output even if empty - don't add fallback message here
                return Result<string>.Success(output);
            }
            else
            {
                var errorMessage = string.IsNullOrEmpty(error) ? $"Script failed with exit code {process.ExitCode}" : error;
                _logger.LogError("Script execution failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);
                return Result<string>.Failure(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing script");
            return Result<string>.Failure($"Script execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes markdown code fences and other formatting from the script.
    /// </summary>
    private static string CleanScript(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
            return script;

        var lines = script.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var cleanedLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip markdown code fence lines
            if (trimmedLine.StartsWith("```"))
                continue;

            // Skip empty lines and comments that are just formatting
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            cleanedLines.Add(line.TrimEnd());
        }

        return string.Join('\n', cleanedLines);
    }
}
