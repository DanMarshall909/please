# PowerShell script to install pls command for Windows
# Run this script to set up the pls alias

param(
    [switch]$SystemWide = $false
)

$pleasePath = "C:\Code\please"
$releasesPath = "C:\Code\please\releases"

Write-Host "🚀 Setting up 'pls' command for Windows..." -ForegroundColor Cyan
Write-Host ""

# Check if files exist
if (-not (Test-Path "$pleasePath\pls.bat")) {
    Write-Error "pls.bat not found in $pleasePath"
    exit 1
}

if (-not (Test-Path "$pleasePath\pls.ps1")) {
    Write-Error "pls.ps1 not found in $pleasePath"
    exit 1
}

# Create releases directory if it doesn't exist
if (-not (Test-Path $releasesPath)) {
    Write-Host "📁 Creating releases directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $releasesPath -Force | Out-Null
}

# Copy scripts to releases directory
Write-Host "📋 Copying pls scripts to releases directory..." -ForegroundColor Yellow
Copy-Item "$pleasePath\pls.bat" "$releasesPath\pls.bat" -Force
Copy-Item "$pleasePath\pls.ps1" "$releasesPath\pls.ps1" -Force

# Add to PATH if not already there
$scope = if ($SystemWide) { "Machine" } else { "User" }
$currentPath = [Environment]::GetEnvironmentVariable("PATH", $scope)

if ($currentPath -notlike "*$releasesPath*") {
    Write-Host "🔧 Adding $releasesPath to $scope PATH..." -ForegroundColor Yellow
    
    if ($SystemWide) {
        Write-Host "⚠️  Adding to system PATH requires Administrator privileges" -ForegroundColor Red
    }
    
    try {
        $newPath = if ($currentPath.EndsWith(";")) { 
            $currentPath + $releasesPath 
        } else { 
            $currentPath + ";" + $releasesPath 
        }
        
        [Environment]::SetEnvironmentVariable("PATH", $newPath, $scope)
        Write-Host "✅ Successfully added to PATH" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to update PATH: $($_.Exception.Message)"
        Write-Host "💡 Try running as Administrator for system-wide installation" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "✅ $releasesPath is already in PATH" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎉 Installation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Usage examples:" -ForegroundColor Yellow
Write-Host "  pls get current time" -ForegroundColor Cyan
Write-Host "  pls list running services" -ForegroundColor Cyan
Write-Host "  pls create backup script for my documents" -ForegroundColor Cyan
Write-Host "  pls find files older than 7 days" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Note: You may need to restart your terminal or Command Prompt" -ForegroundColor Yellow
Write-Host "    for the 'pls' command to be available." -ForegroundColor Yellow