namespace Please.Domain.Services;

/// <summary>
/// Service for platform-specific operations like paths and installation
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// Get the platform-appropriate data directory for Please
    /// </summary>
    string GetDataDirectory();

    /// <summary>
    /// Get the platform-appropriate configuration directory for Please
    /// </summary>
    string GetConfigDirectory();

    /// <summary>
    /// Get the platform-appropriate installation directory for Please
    /// </summary>
    string GetInstallationDirectory();

    /// <summary>
    /// Get the current executable path
    /// </summary>
    string GetCurrentExecutablePath();

    /// <summary>
    /// Check if the application is installed (vs running portable)
    /// </summary>
    bool IsInstalled();

    /// <summary>
    /// Install the application to the platform-appropriate location
    /// </summary>
    Task<bool> InstallAsync();

    /// <summary>
    /// Add the installation directory to the system PATH
    /// </summary>
    Task<bool> AddToPathAsync();

    /// <summary>
    /// Check if the installation directory is in PATH
    /// </summary>
    bool IsInPath();

    /// <summary>
    /// Get the platform name (Windows, Linux, macOS)
    /// </summary>
    string GetPlatformName();

    /// <summary>
    /// Get the expected executable name for this platform
    /// </summary>
    string GetExecutableName();
}