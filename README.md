# Please v6 - C# Rewrite Progress

**Please** is being rewritten in C# using a simple clean architecture. The goal is a small, single-file executable that runs on Windows, Linux and macOS.

## Current Progress
- Core `Result` pattern implemented with mapping helpers
- Strongly typed ID base class and first ID types
- Unit tests cover the new classes with plain English names
- Presentation layer will remain untested

## Project Structure
```text
src/Domain/        # Core domain logic
src/Application/   # Services
src/Infrastructure/ # Providers and storage
src/Presentation/   # CLI commands
```

## Building
```bash
dotnet test --collect:"XPlat Code Coverage" --no-build
```

More features will be migrated from the Go implementation step by step.
