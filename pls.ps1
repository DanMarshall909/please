#!/usr/bin/env pwsh
$exe = "C:\code\please\src\Presentation\Please.Console\bin\Debug\net9.0\win-x64\Please.exe"
dir $exe
Push-Location "C:\Code\please"
try {
  if (Test-Path $exe) {
    & $exe $args
  }
  else {
    dotnet run --project src/Presentation/Please.Console -- $args
  }
}
finally {
  Pop-Location
}
