#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Installation script for Please v6 - Downloads and installs the latest release
.DESCRIPTION
    This script downloads the appropriate Please executable for your platform
    and optionally installs it to your system PATH.
.PARAMETER Version
    Specific version to install (e.g., "v6.0.1"). If not specified, installs the latest release.
.PARAMETER InstallPath
    Custom installation path. If not specified, uses platform defaults.
.PARAMETER Portable
    Download as portable executable without installing to system.
.PARAMETER Force
    Force reinstallation even if Please is already installed.
.EXAMPLE
    .\install.ps1
    .\install.ps1 -Version v6.0.1
    .\install.ps1 -Portable
    .\install.ps1 -InstallPath "C:\Tools"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "latest",

    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "",

    [Parameter(Mandatory=$false)]
    [switch]$Portable = $false,

    [Parameter(Mandatory=$false)]
    [switch]$Force = $false
)

# Color output functions
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Header { param($Message) Write-Host "`n🚀 $Message" -ForegroundColor Magenta }

function Get-PlatformInfo {
    $os = "unknown"
    $arch = "unknown"
    
    if ($IsWindows -or $PSVersionTable.PSVersion.Major -le 5) {
        $os = "windows"
    }
    elseif ($IsLinux) {
        $os = "linux"
    }
    elseif ($IsMacOS) {
        $os = "macos"
    }
    
    $arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture) {
        "X64" { "x64" }
        "Arm64" { "arm64" }
        default { "x64" }
    }
    
    return @{
        OS = $os
        Architecture = $arch
        ExecutableName = if ($os -eq "windows") { "please.exe" } else { "please" }
    }
}

function Get-LatestRelease {
    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/repos/DanMarshall909/please/releases/latest"
        return $response.tag_name
    }
    catch {
        Write-Error "Failed to fetch latest release information: $($_.Exception.Message)"
        return $null
    }
}

function Get-DefaultInstallPath {
    param($Platform)
    
    switch ($Platform.OS) {
        "windows" {
            return Join-Path $env:LOCALAPPDATA "Programs\Please"
        }
        "linux" {
            return Join-Path $env:HOME ".local/bin"
        }
        "macos" {
            return Join-Path $env:HOME ".local/bin"
        }
        default {
            return "./please"
        }
    }
}

function Download-Please {
    param(
        [string]$Version,
        [object]$Platform,
        [string]$OutputPath
    )
    
    $assetName = "please-$($Platform.OS)-$($Platform.Architecture)"
    if ($Platform.OS -eq "windows") {
        $assetName += ".exe"
    }
    
    $downloadUrl = "https://github.com/DanMarshall909/please/releases/download/$Version/$assetName"
    
    Write-Info "Downloading $assetName..."
    Write-Info "URL: $downloadUrl"
    
    try {
        # Create directory if it doesn't exist
        $directory = Split-Path -Path $OutputPath -Parent
        if (![string]::IsNullOrEmpty($directory) -and !(Test-Path $directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        
        Invoke-WebRequest -Uri $downloadUrl -OutFile $OutputPath -UseBasicParsing
        
        # Set executable permissions on Unix systems
        if ($Platform.OS -ne "windows") {
            chmod +x $OutputPath 2>$null
        }
        
        return $true
    }
    catch {
        Write-Error "Download failed: $($_.Exception.Message)"
        return $false
    }
}

function Add-ToPath {
    param([string]$Path)
    
    $platform = Get-PlatformInfo
    
    if ($platform.OS -eq "windows") {
        # Windows - add to user PATH
        $currentPath = [Environment]::GetEnvironmentVariable("PATH", "User")
        if ($currentPath -notlike "*$Path*") {
            $newPath = if ($currentPath.EndsWith(";")) { 
                $currentPath + $Path 
            } else { 
                $currentPath + ";" + $Path 
            }
            [Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
            Write-Success "Added to Windows user PATH"
        }
        else {
            Write-Info "Already in Windows PATH"
        }
    }
    else {
        # Unix - add to shell profiles
        $profileFiles = @(
            "$env:HOME/.bashrc",
            "$env:HOME/.zshrc", 
            "$env:HOME/.profile"
        )
        
        $exportLine = "export PATH=`"$Path:`$PATH`""
        $added = $false
        
        foreach ($profileFile in $profileFiles) {
            if (Test-Path $profileFile) {
                $content = Get-Content $profileFile -Raw -ErrorAction SilentlyContinue
                if ($content -notlike "*$Path*") {
                    Add-Content -Path $profileFile -Value "`n$exportLine"
                    Write-Success "Added to $(Split-Path -Leaf $profileFile)"
                    $added = $true
                    break
                }
                else {
                    Write-Info "Already in $(Split-Path -Leaf $profileFile)"
                    $added = $true
                    break
                }
            }
        }
        
        if (-not $added) {
            # Create .profile if no shell config exists
            Add-Content -Path "$env:HOME/.profile" -Value $exportLine
            Write-Success "Created .profile with PATH export"
        }
    }
}

# Main installation logic
function Main {
    Write-Header "Please v6 Installation Script"
    
    # Get platform information
    $platform = Get-PlatformInfo()
    Write-Info "Detected platform: $($platform.OS)-$($platform.Architecture)"
    
    # Determine version to install
    $targetVersion = $Version
    if ($Version -eq "latest") {
        Write-Info "Fetching latest release information..."
        $targetVersion = Get-LatestRelease
        if (-not $targetVersion) {
            Write-Error "Could not determine latest version"
            exit 1
        }
    }
    Write-Info "Target version: $targetVersion"
    
    # Determine installation path
    if ($Portable) {
        $installDir = "."
        $installPath = Join-Path $installDir $platform.ExecutableName
        Write-Info "Portable mode - downloading to current directory"
    }
    else {
        $installDir = if ($InstallPath) { $InstallPath } else { Get-DefaultInstallPath $platform }
        $installPath = Join-Path $installDir $platform.ExecutableName
        Write-Info "Installation directory: $installDir"
    }
    
    # Check if already installed
    if ((Test-Path $installPath) -and -not $Force) {
        Write-Warning "Please is already installed at $installPath"
        Write-Info "Use -Force to reinstall or -Portable to download to current directory"
        
        # Test the existing installation
        try {
            $version = & $installPath --version 2>$null
            Write-Info "Current installation: $version"
        }
        catch {
            Write-Warning "Existing installation may be corrupted"
        }
        
        exit 0
    }
    
    # Download Please
    Write-Header "Downloading Please $targetVersion"
    $downloadSuccess = Download-Please -Version $targetVersion -Platform $platform -OutputPath $installPath
    
    if (-not $downloadSuccess) {
        Write-Error "Installation failed"
        exit 1
    }
    
    Write-Success "Downloaded Please to $installPath"
    
    # Test the installation
    try {
        Write-Info "Testing installation..."
        $versionOutput = & $installPath --version
        Write-Success "Installation test passed: $versionOutput"
    }
    catch {
        Write-Error "Installation test failed: $($_.Exception.Message)"
        exit 1
    }
    
    # Add to PATH if not portable
    if (-not $Portable) {
        Write-Header "Configuring PATH"
        Add-ToPath $installDir
    }
    
    # Installation complete
    Write-Header "Installation Complete!"
    Write-Success "Please v$targetVersion installed successfully"
    
    if ($Portable) {
        Write-Info "Portable installation ready:"
        Write-Host "  ./$($platform.ExecutableName) get current time" -ForegroundColor Cyan
        Write-Host "  ./$($platform.ExecutableName) --help" -ForegroundColor Cyan
    }
    else {
        Write-Info "Please is now available system-wide:"
        Write-Host "  please get current time" -ForegroundColor Cyan
        Write-Host "  please list running services" -ForegroundColor Cyan
        Write-Host "  please --help" -ForegroundColor Cyan
        Write-Warning "You may need to restart your terminal for PATH changes to take effect"
    }
    
    Write-Info "`nNext steps:"
    Write-Host "  1. Configure an AI provider (run: please --help)" -ForegroundColor Gray
    Write-Host "  2. Start generating scripts with natural language!" -ForegroundColor Gray
}

# Run the installer
try {
    Main
}
catch {
    Write-Error "Installation failed with error: $($_.Exception.Message)"
    exit 1
}