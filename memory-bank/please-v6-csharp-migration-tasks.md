# Please v6 C# Migration Tasks

## Overview

This document outlines the tasks needed to convert the Go implementation in the `legacy/` folder to C# following Clean Architecture patterns, TDD principles, and the existing domain model in `src/`.

## 🚦 Project Structure Status

- ✅ Clean Architecture foundation established
- ✅ Domain entities and enums implemented
- ✅ Application layer with CQRS/MediatR setup (to be removed)
- ✅ Test project structure established
- ✅ Script validation service interface and tests created

## 🚨 STRATEGIC FOCUS

- **Go enhancement and UI testing are DEFERRED**. No further Go development is planned; the Go codebase is preserved in `legacy/` for reference/specification only.
- **C# Result Pattern migration is the SOLE active track**. All implementation, testing, and documentation must align with this direction.

## Core Business Logic Migration Tasks

- Script Validation Service Implementation
- AI Provider Integration
- Dependency Injection Setup

## Infrastructure Layer Tasks

- Register script validation, AI provider, configuration, and repository services
- Add structured logging, error tracking, and performance counters
- Add HTTP client for AI provider APIs, file system/process abstractions

## Application Layer Enhancements

- Add/expand commands and queries (e.g., ValidateScriptCommand, ExecuteScriptCommand)
- Add comprehensive input validation and error handling

## Testing Strategy

- Domain and application unit tests (expand coverage)
- Infrastructure integration tests
- End-to-end testing scenarios
- Performance testing for AI provider calls

## Migration-Specific Tasks

- Analyze Go code for specification only
- Map Go error handling to C# Result pattern
- Migrate data/configuration as needed
- **[NEW] Fix Go build and ensure a working legacy release is available**
  - Update Go build scripts as needed (build.bat, build.sh)
  - Verify all Go tests pass
  - Produce a working Go binary for reference/legacy users

## Presentation Layer Tasks

- Enhance Program.cs with CLI interface
- Add command-line argument parsing and interactive mode
- Add error display and user feedback

## Quality and Documentation

- Add code coverage reporting, static analysis, automated formatting, XML docs
- Update README and developer setup guides for C# implementation

## Deployment and Distribution

- Add CI/CD pipeline configuration, NuGet packaging, self-contained deployment options

## Implementation Priority Order

1. **Phase 1 (Core Business Logic)**
   - Script Validation Service Implementation
   - AI Provider Integration
   - Dependency Injection Setup
2. **Phase 2 (Application Features)**
   - Configuration Management
   - Script Repository Implementation
   - Error Handling and Validation
3. **Phase 3 (User Experience)**
   - Console Application enhancements
   - Interactive Interface
   - Localization System
4. **Phase 4 (Quality and Polish)**
   - Test Coverage Enhancement
   - Performance Optimization
   - Documentation and Deployment

## Success Criteria

- ✅ All existing Go functionality ported to C#
- ✅ Performance equal to or better than Go implementation
- ✅ 90%+ test coverage on business logic
- ✅ Clean Architecture principles maintained
- ✅ Cross-platform compatibility maintained
- ✅ Existing user workflows preserved

## Notes

- The Go codebase in `legacy/` serves as the specification for behavior
- All new C# code should follow established patterns in `src/`
- TDD approach required for all business logic
- Focus on maintainability and extensibility over rapid migration
- Validate each component works before moving to the next

---

_Go enhancement is deferred as of June 21, 2025. C# Result Pattern migration is the only active track._
