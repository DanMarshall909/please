using Please.Domain.Interfaces;
using Please.Domain.Services;

namespace Please.Application.Services;

/// <summary>
/// Service for handling application installation and first-run setup
/// </summary>
public class InstallationService
{
    private readonly IPlatformService _platformService;
    private readonly IConsoleUIService _uiService;

    public InstallationService(IPlatformService platformService, IConsoleUIService uiService)
    {
        _platformService = platformService;
        _uiService = uiService;
    }

    /// <summary>
    /// Check if this is the first run and handle installation if needed
    /// </summary>
    public async Task HandleFirstRunAsync()
    {
        // Skip if already installed
        if (_platformService.IsInstalled())
            return;

        // Check if this is a first run by looking for marker file
        var configDir = _platformService.GetConfigDirectory();
        var markerFile = Path.Combine(configDir, ".first-run-complete");

        if (File.Exists(markerFile))
            return; // Not first run

        // This is the first run - show installation prompt
        await ShowInstallationPromptAsync();

        // Create marker file to indicate first run is complete
        Directory.CreateDirectory(configDir);
        await File.WriteAllTextAsync(markerFile, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    private async Task ShowInstallationPromptAsync()
    {
        var platform = _platformService.GetPlatformName();
        var installDir = _platformService.GetInstallationDirectory();
        
        Console.WriteLine();
        Console.WriteLine("🎉 Welcome to Please v6 - AI-Powered Script Generator!");
        Console.WriteLine();
        Console.WriteLine($"You're currently running Please as a portable application on {platform}.");
        Console.WriteLine("Would you like to install it to your system for easier access?");
        Console.WriteLine();
        Console.WriteLine("Benefits of installing:");
        Console.WriteLine("• Available from any directory by typing 'please'");
        Console.WriteLine("• Automatic PATH configuration");
        Console.WriteLine($"• Installed to: {installDir}");
        Console.WriteLine("• Settings stored in platform-appropriate location");
        Console.WriteLine();

        var options = new[] { "✅ Install to system", "📁 Keep as portable", "❌ Skip (ask later)" };
        var choice = _uiService.DisplayInteractiveMenu(options);

        switch (choice)
        {
            case 0: // Install
                await PerformInstallationAsync();
                break;
            case 1: // Keep portable
                Console.WriteLine();
                Console.WriteLine("👍 Keeping as portable application.");
                Console.WriteLine("You can install later by running: please --install");
                break;
            case 2: // Skip
                Console.WriteLine();
                Console.WriteLine("⏭️  Installation skipped. I'll ask again next time.");
                Console.WriteLine("You can install anytime by running: please --install");
                // Don't create the marker file so we ask again
                return;
        }

        Console.WriteLine();
    }

    private async Task PerformInstallationAsync()
    {
        Console.WriteLine();
        Console.WriteLine("🔧 Installing Please to your system...");

        try
        {
            // Install the executable
            var installSuccess = await _platformService.InstallAsync();
            if (!installSuccess)
            {
                Console.WriteLine("❌ Failed to copy executable. You may need administrator privileges.");
                return;
            }
            Console.WriteLine("✅ Executable installed successfully");

            // Add to PATH
            var pathSuccess = await _platformService.AddToPathAsync();
            if (pathSuccess)
            {
                Console.WriteLine("✅ Added to system PATH");
            }
            else
            {
                Console.WriteLine("⚠️  Could not automatically add to PATH. You may need to do this manually.");
            }

            // Create data and config directories
            Directory.CreateDirectory(_platformService.GetDataDirectory());
            Directory.CreateDirectory(_platformService.GetConfigDirectory());

            Console.WriteLine();
            Console.WriteLine("🎉 Installation complete!");
            Console.WriteLine();
            Console.WriteLine("You can now use 'please' from any directory:");
            Console.WriteLine("  please get current time");
            Console.WriteLine("  please list running services");
            Console.WriteLine("  please create backup script");
            Console.WriteLine();
            
            if (!pathSuccess)
            {
                Console.WriteLine($"💡 Add this to your PATH manually: {_platformService.GetInstallationDirectory()}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("📝 Note: You may need to restart your terminal for PATH changes to take effect.");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Installation failed: {ex.Message}");
            Console.WriteLine("You can try running with administrator privileges or use the portable version.");
        }
    }

    /// <summary>
    /// Force installation (for --install command)
    /// </summary>
    public async Task ForceInstallAsync()
    {
        if (_platformService.IsInstalled())
        {
            Console.WriteLine("✅ Please is already installed to the system.");
            Console.WriteLine($"Location: {_platformService.GetInstallationDirectory()}");
            
            if (!_platformService.IsInPath())
            {
                Console.WriteLine("⚠️  Not found in PATH. Adding now...");
                await _platformService.AddToPathAsync();
                Console.WriteLine("✅ Added to PATH");
            }
            return;
        }

        await PerformInstallationAsync();
    }

    /// <summary>
    /// Show installation status
    /// </summary>
    public void ShowStatus()
    {
        var isInstalled = _platformService.IsInstalled();
        var isInPath = _platformService.IsInPath();
        var platform = _platformService.GetPlatformName();
        var executablePath = _platformService.GetCurrentExecutablePath();
        var installDir = _platformService.GetInstallationDirectory();
        var configDir = _platformService.GetConfigDirectory();
        var dataDir = _platformService.GetDataDirectory();

        Console.WriteLine();
        Console.WriteLine("📊 Please Installation Status");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"Platform: {platform}");
        Console.WriteLine($"Current executable: {executablePath}");
        Console.WriteLine($"Installation status: {(isInstalled ? "✅ Installed" : "📁 Portable")}");
        Console.WriteLine($"In PATH: {(isInPath ? "✅ Yes" : "❌ No")}");
        Console.WriteLine();
        Console.WriteLine("Directories:");
        Console.WriteLine($"• Install: {installDir}");
        Console.WriteLine($"• Config:  {configDir}");
        Console.WriteLine($"• Data:    {dataDir}");
        Console.WriteLine();

        if (!isInstalled)
        {
            Console.WriteLine("💡 Run 'please --install' to install to your system");
        }
        else if (!isInPath)
        {
            Console.WriteLine("💡 Run 'please --install' to fix PATH configuration");
        }
    }
}