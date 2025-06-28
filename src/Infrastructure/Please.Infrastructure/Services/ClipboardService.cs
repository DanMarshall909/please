using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Services;

/// <summary>
/// Cross-platform clipboard service implementation
/// </summary>
public class ClipboardService : IClipboardService
{
    private readonly ILogger<ClipboardService> _logger;
    private readonly bool _isSupported;

    public ClipboardService(ILogger<ClipboardService> logger)
    {
        _logger = logger;
        _isSupported = CheckClipboardSupport();
    }

    public async Task<bool> SetTextAsync(string text)
    {
        if (!_isSupported)
        {
            _logger.LogWarning("Clipboard operations are not supported on this platform");
            return false;
        }

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await SetTextWindowsAsync(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await SetTextLinuxAsync(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await SetTextMacAsync(text);
            }

            _logger.LogError("Unsupported operating system for clipboard operations");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set clipboard text");
            return false;
        }
    }

    public async Task<string?> GetTextAsync()
    {
        if (!_isSupported)
        {
            _logger.LogWarning("Clipboard operations are not supported on this platform");
            return null;
        }

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await GetTextWindowsAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await GetTextLinuxAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await GetTextMacAsync();
            }

            _logger.LogError("Unsupported operating system for clipboard operations");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get clipboard text");
            return null;
        }
    }

    public bool IsSupported() => _isSupported;

    private bool CheckClipboardSupport()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return true; // Windows always supports clipboard via PowerShell
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Check if xclip or xsel is available
                return IsCommandAvailable("xclip") || IsCommandAvailable("xsel") || IsCommandAvailable("wl-copy");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS always has pbcopy/pbpaste
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool IsCommandAvailable(string command)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(1000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> SetTextWindowsAsync(string text)
    {
        // Use PowerShell Set-Clipboard cmdlet
        var script = $"Set-Clipboard -Value @'\n{text.Replace("'", "''")}\n'@";
        return await RunPowerShellCommandAsync(script);
    }

    private async Task<string?> GetTextWindowsAsync()
    {
        // Use PowerShell Get-Clipboard cmdlet
        var script = "Get-Clipboard -Raw";
        var (success, output) = await RunPowerShellCommandWithOutputAsync(script);
        return success ? output?.TrimEnd('\r', '\n') : null;
    }

    private async Task<bool> SetTextLinuxAsync(string text)
    {
        // Try different clipboard utilities in order of preference
        if (IsCommandAvailable("wl-copy"))
        {
            // Wayland
            return await RunCommandWithInputAsync("wl-copy", "", text);
        }
        else if (IsCommandAvailable("xclip"))
        {
            // X11 with xclip
            return await RunCommandWithInputAsync("xclip", "-selection clipboard", text);
        }
        else if (IsCommandAvailable("xsel"))
        {
            // X11 with xsel
            return await RunCommandWithInputAsync("xsel", "--clipboard --input", text);
        }

        _logger.LogError("No clipboard utility found on Linux. Install xclip, xsel, or wl-clipboard");
        return false;
    }

    private async Task<string?> GetTextLinuxAsync()
    {
        // Try different clipboard utilities in order of preference
        if (IsCommandAvailable("wl-paste"))
        {
            // Wayland
            var (success, output) = await RunCommandWithOutputAsync("wl-paste", "");
            return success ? output : null;
        }
        else if (IsCommandAvailable("xclip"))
        {
            // X11 with xclip
            var (success, output) = await RunCommandWithOutputAsync("xclip", "-selection clipboard -o");
            return success ? output : null;
        }
        else if (IsCommandAvailable("xsel"))
        {
            // X11 with xsel
            var (success, output) = await RunCommandWithOutputAsync("xsel", "--clipboard --output");
            return success ? output : null;
        }

        _logger.LogError("No clipboard utility found on Linux. Install xclip, xsel, or wl-clipboard");
        return null;
    }

    private async Task<bool> SetTextMacAsync(string text)
    {
        // Use pbcopy on macOS
        return await RunCommandWithInputAsync("pbcopy", "", text);
    }

    private async Task<string?> GetTextMacAsync()
    {
        // Use pbpaste on macOS
        var (success, output) = await RunCommandWithOutputAsync("pbpaste", "");
        return success ? output : null;
    }

    private async Task<bool> RunPowerShellCommandAsync(string script)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run PowerShell command");
            return false;
        }
    }

    private async Task<(bool success, string? output)> RunPowerShellCommandWithOutputAsync(string script)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return (process.ExitCode == 0, output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run PowerShell command");
            return (false, null);
        }
    }

    private async Task<bool> RunCommandWithInputAsync(string command, string arguments, string input)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.StandardInput.WriteAsync(input);
            process.StandardInput.Close();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run command: {Command}", command);
            return false;
        }
    }

    private async Task<(bool success, string? output)> RunCommandWithOutputAsync(string command, string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return (process.ExitCode == 0, output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run command: {Command}", command);
            return (false, null);
        }
    }
}