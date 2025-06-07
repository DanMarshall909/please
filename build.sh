#!/bin/bash

# Please - Cross-Platform Build Script
# Builds binaries for all supported platforms

echo "🤖 Building Please for all platforms..."

# Create releases directory
mkdir -p releases

# Build for all platforms
echo "Building Windows AMD64..."
GOOS=windows GOARCH=amd64 go build -ldflags="-s -w" -o releases/please-windows-amd64.exe main.go

echo "Building Linux AMD64..."
GOOS=linux GOARCH=amd64 go build -ldflags="-s -w" -o releases/please-linux-amd64 main.go

echo "Building Linux ARM64..."
GOOS=linux GOARCH=arm64 go build -ldflags="-s -w" -o releases/please-linux-arm64 main.go

echo "Building macOS AMD64..."
GOOS=darwin GOARCH=amd64 go build -ldflags="-s -w" -o releases/please-macos-amd64 main.go

echo "Building macOS ARM64..."
GOOS=darwin GOARCH=arm64 go build -ldflags="-s -w" -o releases/please-macos-arm64 main.go

echo "✅ Build complete! Binaries available in ./releases/"
ls -la releases/
