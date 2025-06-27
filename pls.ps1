#!/usr/bin/env pwsh
Push-Location "C:\Code\please"
try {
    dotnet run --project src/Presentation/Please.Console -- $args
} finally {
    Pop-Location
}