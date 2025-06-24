
.PHONY: all clean test build

# Default target
all: build test

# Variables
DOTNET_CONFIGURATION ?= Debug
GO_BINDIR = bin
GOOS ?= $(shell go env GOOS)
GOARCH ?= $(shell go env GOARCH)

# Build both .NET and Go
build: build-dotnet build-go

# Build .NET solution
build-dotnet:
	@echo "Building .NET solution..."
	dotnet build --configuration $(DOTNET_CONFIGURATION)

# Build Go project
build-go:
	@echo "Building Go project..."
	@mkdir -p $(GO_BINDIR)
	go build -o $(GO_BINDIR)/ ./...

# Run all tests
test: test-dotnet test-go

# Run .NET tests
test-dotnet:
	@echo "Running .NET tests..."
	dotnet test --configuration $(DOTNET_CONFIGURATION) --no-build

# Run Go tests
test-go:
	@echo "Running Go tests..."
	go test ./...

# Clean build outputs
clean:
	@echo "Cleaning build outputs..."
	dotnet clean
	rm -rf $(GO_BINDIR)
	rm -rf */bin */obj