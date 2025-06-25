using System.Diagnostics;
using System.Text;
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

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-ExecutionPolicy Bypass -NoProfile -Command -",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            process.Start();

            // Send the script to PowerShell
            await process.StandardInput.WriteAsync(script);
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
}
