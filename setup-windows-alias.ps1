# PowerShell script to add pls command to Windows PATH
# Run this as Administrator in PowerShell

$pleasePath = "C:\Code\please"
$currentPath = [Environment]::GetEnvironmentVariable("PATH", "Machine")

if ($currentPath -notlike "*$pleasePath*") {
    Write-Host "Adding C:\Code\please to system PATH..." -ForegroundColor Green
    [Environment]::SetEnvironmentVariable("PATH", $currentPath + ";$pleasePath", "Machine")
    Write-Host "✅ Added to system PATH. Restart your terminal to use 'pls' command." -ForegroundColor Green
} else {
    Write-Host "✅ C:\Code\please is already in PATH" -ForegroundColor Green
}

Write-Host ""
Write-Host "You can now use:" -ForegroundColor Yellow
Write-Host "  pls get current time" -ForegroundColor Cyan
Write-Host "  pls list running services" -ForegroundColor Cyan
Write-Host "  pls create backup script" -ForegroundColor Cyan