using Please.Domain.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Please.Infrastructure.Services;

/// <summary>
/// Platform-specific service implementation for Windows, Linux, and macOS
/// </summary>
public class PlatformService : IPlatformService
{
    public string GetDataDirectory()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Please")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "please");
    }

    public string GetConfigDirectory()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Please")
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Please")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "please");
    }

    public string GetInstallationDirectory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Please");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
        }
        else // Linux
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
        }
    }

    public string GetCurrentExecutablePath()
    {
        return Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
    }

    public bool IsInstalled()
    {
        var currentPath = GetCurrentExecutablePath();
        var installDir = GetInstallationDirectory();
        
        return currentPath.StartsWith(installDir, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> InstallAsync()
    {
        try
        {
            var currentPath = GetCurrentExecutablePath();
            var installDir = GetInstallationDirectory();
            var executableName = GetExecutableName();
            var targetPath = Path.Combine(installDir, executableName);

            // Create installation directory
            Directory.CreateDirectory(installDir);

            // Copy executable
            if (File.Exists(targetPath))
            {
                // Try to replace existing file
                try
                {
                    File.Delete(targetPath);
                }
                catch
                {
                    // If we can't delete, try to backup and replace
                    var backupPath = targetPath + ".backup";
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(targetPath, backupPath);
                }
            }

            await File.ReadAllBytesAsync(currentPath).ContinueWith(async data =>
                await File.WriteAllBytesAsync(targetPath, data.Result));

            // Set executable permissions on Unix systems
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var chmod = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{targetPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                chmod?.WaitForExit();
            }

            return File.Exists(targetPath);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddToPathAsync()
    {
        try
        {
            var installDir = GetInstallationDirectory();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Add to Windows user PATH
                var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                
                if (!currentPath.Contains(installDir))
                {
                    var newPath = string.IsNullOrEmpty(currentPath) 
                        ? installDir 
                        : $"{currentPath};{installDir}";
                    
                    Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                }
                return true;
            }
            else
            {
                // Add to Unix shell profile
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var profileFiles = new[]
                {
                    Path.Combine(homeDir, ".bashrc"),
                    Path.Combine(homeDir, ".zshrc"),
                    Path.Combine(homeDir, ".profile")
                };

                var exportLine = $"export PATH=\"{installDir}:$PATH\"";
                
                foreach (var profileFile in profileFiles)
                {
                    if (File.Exists(profileFile))
                    {
                        var content = await File.ReadAllTextAsync(profileFile);
                        if (!content.Contains(installDir))
                        {
                            await File.AppendAllTextAsync(profileFile, $"\n{exportLine}\n");
                        }
                        break; // Only add to the first existing profile file
                    }
                }

                // If no profile exists, create .profile
                if (!profileFiles.Any(File.Exists))
                {
                    await File.WriteAllTextAsync(Path.Combine(homeDir, ".profile"), $"{exportLine}\n");
                }

                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public bool IsInPath()
    {
        var installDir = GetInstallationDirectory();
        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        
        return pathVar.Split(Path.PathSeparator)
            .Any(p => string.Equals(p.Trim(), installDir, StringComparison.OrdinalIgnoreCase));
    }

    public string GetPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux";
        else
            return "Unknown";
    }

    public string GetExecutableName()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "please.exe" : "please";
    }
}