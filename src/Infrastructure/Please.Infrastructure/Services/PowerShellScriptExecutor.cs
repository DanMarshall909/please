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

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    outputBuilder.AppendLine(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    errorBuilder.AppendLine(args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Send the script to PowerShell
            await process.StandardInput.WriteAsync(script);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close();

            await process.WaitForExitAsync();

            var output = outputBuilder.ToString().Trim();
            var error = errorBuilder.ToString().Trim();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Script executed successfully with exit code {ExitCode}", process.ExitCode);
                return Result<string>.Success(string.IsNullOrEmpty(output) ? "Script completed successfully" : output);
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
