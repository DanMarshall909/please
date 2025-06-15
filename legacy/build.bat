@echo off
REM Please - Cross-Platform Build Script for Windows
REM Builds binaries for all supported platforms

echo 🤖 Building Please for all platforms...

REM Create releases directory
if not exist releases mkdir releases

REM Build for all platforms
echo Building Windows AMD64...
set GOOS=windows&& set GOARCH=amd64&& go build -ldflags="-s -w" -o releases/please-windows-amd64.exe main.go

echo Building Linux AMD64...
set GOOS=linux&& set GOARCH=amd64&& go build -ldflags="-s -w" -o releases/please-linux-amd64 main.go

echo Building Linux ARM64...
set GOOS=linux&& set GOARCH=arm64&& go build -ldflags="-s -w" -o releases/please-linux-arm64 main.go

echo Building macOS AMD64...
set GOOS=darwin&& set GOARCH=amd64&& go build -ldflags="-s -w" -o releases/please-macos-amd64 main.go

echo Building macOS ARM64...
set GOOS=darwin&& set GOARCH=arm64&& go build -ldflags="-s -w" -o releases/please-macos-arm64 main.go

echo ✅ Build complete! Binaries available in ./releases/
dir releases
