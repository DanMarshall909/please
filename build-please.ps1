# Build and Install Please Tool
# This script builds the please tool and demonstrates natural language usage

Write-Host "🔨 Building please tool..." -ForegroundColor Green

# Build the project
dotnet build src/Presentation/Please.Console -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    
    # Show the built executable location
    $buildPath = "src/Presentation/Please.Console/bin/Release/net8.0/win-x64/please.exe"
    
    if (Test-Path $buildPath) {
        Write-Host "📍 Executable location: $buildPath" -ForegroundColor Cyan
        
        Write-Host "`n🎯 Natural Language Usage Examples:" -ForegroundColor Yellow
        Write-Host "  $buildPath find files older than 3 days"
        Write-Host "  $buildPath create a backup script for my documents"
        Write-Host "  $buildPath list all running services on this computer"
        Write-Host "  $buildPath generate a script to clean temporary files"
        Write-Host "  $buildPath show system information and memory usage"
        Write-Host "  $buildPath create a PowerShell script to monitor disk space"
        
        Write-Host "`n💡 Notes:" -ForegroundColor Magenta
        Write-Host "  • No quotes needed around the request"
        Write-Host "  • Use natural English sentences"
        Write-Host "  • The AI will interpret your intent"
        Write-Host "  • System will validate and suggest edits"
        
        Write-Host "`n🚀 To make it globally available:" -ForegroundColor Cyan
        Write-Host "  1. Copy please.exe to a folder in your PATH"
        Write-Host "  2. Or add the build folder to your PATH"
        Write-Host "  3. Then you can use: please find files older than 3 days"
        
    } else {
        Write-Host "❌ Could not find built executable at $buildPath" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
}

Write-Host "`n✨ Ready to use natural language script generation!" -ForegroundColor Green