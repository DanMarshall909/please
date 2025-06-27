@echo off
pushd "C:\Code\please"
dotnet run --project src/Presentation/Please.Console -- %*
popd