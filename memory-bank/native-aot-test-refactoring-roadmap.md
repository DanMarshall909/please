# Native AOT Test Refactoring Roadmap

## 🎯 OBJECTIVE
Transform the duplicated, hacky Native AOT test setup into clean, maintainable, enterprise-grade test architecture.

## 📋 TASK OVERVIEW
- **Task 1**: Complete TestModule Pattern Implementation (15-30 min)
- **Task 2**: Complete AOT Hack Removal & Cleanup (15-30 min)
- **Task 3**: Implement Advanced Test Infrastructure (20-40 min)

**Total Time**: 50-100 minutes | **Current Status**: ❌ Not Started

---

## 📝 COPY/PASTE PROMPTS

### 🔧 Task 1: Complete TestModule Pattern Implementation

**Status**: ❌ Not Started

**Goal**: Eliminate 90% of code duplication by centralizing test registrations

**Prompt to Copy/Paste**:
```
I need to refactor the Native AOT test setup to eliminate code duplication. Currently every test class repeats the same 6-8 lines of service registration.

TASK: Create a TestModule pattern and update ALL test classes to use it.

REQUIREMENTS:
1. Create TestModule.cs in Please.TestUtilities with AddTestDoubles() extension method
2. Update TestSystem.cs to use the new TestModule
3. Refactor ALL test classes to remove duplicated service registrations
4. Ensure all 41 tests still pass
5. Update memory-bank/native-aot-compatibility-for-tests.md to document the new pattern
6. Update memory-bank/strategic-testing-c#-migration-progress.md to mark this completed

FILES TO MODIFY:
- tests/TestUtilities/Please.TestUtilities/TestModule.cs (CREATE)
- tests/TestUtilities/Please.TestUtilities/TestSystem.cs (UPDATE)
- tests/Application.UnitTests/Please.Application.UnitTests/Services/ScriptServiceTests.cs (SIMPLIFY)
- tests/Application.UnitTests/Please.Application.UnitTests/Services/CommandProcessorTests.cs (SIMPLIFY)
- tests/Application.UnitTests/Please.Application.UnitTests/PleaseHostTests.cs (SIMPLIFY)
- tests/Application.IntegrationTests/Please.Application.IntegrationTests/ScriptGenerationIntegrationTests.cs (SIMPLIFY)
- tests/Application.IntegrationTests/Please.Application.IntegrationTests/CommandProcessorIntegrationTests.cs (SIMPLIFY)
- memory-bank/native-aot-compatibility-for-tests.md (UPDATE)
- memory-bank/strategic-testing-c#-migration-progress.md (UPDATE)

VERIFICATION: Run `dotnet test` - all 41 tests must pass

Please implement this completely, test it, and update all documentation.
```

**✅ Completion Criteria**:
- [ ] TestModule.cs created with AddTestDoubles() method
- [ ] TestSystem.cs updated to use TestModule
- [ ] All 5 test classes simplified (no duplicate registrations)
- [ ] All 41 tests pass
- [ ] Documentation updated
- [ ] Git commit created

---

### 🧹 Task 2: Complete AOT Hack Removal & Cleanup

**Status**: ❌ Not Started (Complete Task 1 First)

**Goal**: Remove the ugly DateTime.Now.Ticks hack and implement clean AOT-friendly solution

**Prompt to Copy/Paste**:
```
I need to remove the ugly AOT hack in DependencyInjection.cs and implement a clean, maintainable solution.

TASK: Remove DateTime.Now.Ticks hack and implement proper AOT-friendly registration.

CURRENT PROBLEM: DependencyInjection.cs has unreachable code with dummy implementations - this is a major code smell.

REQUIREMENTS:
1. Remove the entire registerRequiredInterfacesForAot method and dummy classes
2. Implement proper AOT hints using [DynamicallyAccessedMembers] attributes
3. Ensure the AOT compiler still preserves required types
4. Clean up all unused code and suppression attributes
5. Ensure all 41 tests still pass
6. Update memory-bank/native-aot-compatibility-for-tests.md to document the clean solution
7. Update memory-bank/strategic-testing-c#-migration-progress.md to mark this completed

FILES TO MODIFY:
- src/Application/Please.Application/DependencyInjection.cs (CLEAN UP)
- memory-bank/native-aot-compatibility-for-tests.md (UPDATE)
- memory-bank/strategic-testing-c#-migration-progress.md (UPDATE)

VERIFICATION:
1. Run `dotnet test` - all 41 tests must pass
2. Code should have NO unreachable code warnings
3. No DateTime.Now.Ticks hacks
4. No dummy UnusedScriptGenerator classes

Please implement this completely, test it, and update all documentation.
```

**✅ Completion Criteria**:
- [ ] DateTime.Now.Ticks hack completely removed
- [ ] All dummy classes (UnusedScriptGenerator, etc.) removed
- [ ] Proper AOT attributes implemented
- [ ] All 41 tests pass
- [ ] No compiler warnings
- [ ] Documentation updated
- [ ] Git commit created

---

### 🏗️ Task 3: Implement Advanced Test Infrastructure

**Status**: ❌ Not Started (Complete Tasks 1 & 2 First)

**Goal**: Create enterprise-grade test architecture with fluent API and base classes

**Prompt to Copy/Paste**:
```
I need to implement advanced test infrastructure to make testing more maintainable and developer-friendly.

TASK: Create TestServiceBuilder fluent API and base test classes for enterprise-grade testing.

REQUIREMENTS:
1. Create TestServiceBuilder.cs with fluent API for custom test configurations
2. Create ApplicationTestBase.cs abstract base class to reduce boilerplate
3. Refactor complex test scenarios to use the new infrastructure
4. Ensure all 41 tests still pass
5. Add examples and usage documentation
6. Update memory-bank/native-aot-compatibility-for-tests.md with advanced patterns
7. Update memory-bank/strategic-testing-c#-migration-progress.md to mark refactoring complete

FILES TO CREATE:
- tests/TestUtilities/Please.TestUtilities/TestServiceBuilder.cs (CREATE)
- tests/TestUtilities/Please.TestUtilities/ApplicationTestBase.cs (CREATE)

FILES TO ENHANCE:
- tests/Application.UnitTests/Please.Application.UnitTests/Services/ScriptServiceTests.cs (ENHANCE)
- tests/Application.UnitTests/Please.Application.UnitTests/Services/CommandProcessorTests.cs (ENHANCE)
- memory-bank/native-aot-compatibility-for-tests.md (UPDATE)
- memory-bank/strategic-testing-c#-migration-progress.md (UPDATE)

VERIFICATION: Run `dotnet test` - all 41 tests must pass

DESIGN PATTERNS TO IMPLEMENT:
- Builder Pattern for test configuration
- Template Method Pattern for base test classes
- Factory Pattern for service creation
- Fluent Interface for developer experience

Please implement this completely, test it, and update all documentation.
```

**✅ Completion Criteria**:
- [ ] TestServiceBuilder.cs created with fluent API
- [ ] ApplicationTestBase.cs created
- [ ] At least 2 test classes enhanced to use new infrastructure
- [ ] All 41 tests pass
- [ ] Usage examples documented
- [ ] Documentation updated
- [ ] Git commit created

---

## 📊 PROGRESS TRACKING

### Current State
- ❌ **TestModule Pattern**: Not implemented
- ❌ **AOT Hack Removal**: Still using DateTime.Now.Ticks hack
- ❌ **Advanced Infrastructure**: No fluent API or base classes
- ✅ **Tests Working**: All 41 tests pass (baseline established)

### Target State
- ✅ **TestModule Pattern**: Centralized, no duplication
- ✅ **AOT Hack Removal**: Clean, maintainable AOT solution
- ✅ **Advanced Infrastructure**: Enterprise-grade test architecture
- ✅ **Tests Working**: All 41 tests pass

## 🚨 ROLLBACK PLAN

If any task breaks tests:
1. **Stop immediately**
2. Run `git status` to see changes
3. Run `git checkout -- .` to undo changes
4. Re-run `dotnet test` to confirm tests pass
5. Review the task requirements and try again

## ✅ FINAL VERIFICATION

After completing all 3 tasks:
1. Run `dotnet test` - all 41 tests must pass
2. Check `git log --oneline -3` - should see 3 commits
3. Review memory bank files - should be updated
4. Code should have zero duplication and no hacks

## 🎯 SUCCESS METRICS

**Before**:
- 40+ lines of duplicated test registration code
- Ugly DateTime.Now.Ticks hack in production code
- No reusable test infrastructure

**After**:
- 1 centralized TestModule with all registrations
- Clean AOT-friendly production code
- Fluent test builder and base classes
- Enterprise-grade maintainability

---

## 📚 DOCUMENTATION UPDATES REQUIRED

Each task must update:
1. `memory-bank/native-aot-compatibility-for-tests.md` - Technical implementation
2. `memory-bank/strategic-testing-c#-migration-progress.md` - Progress tracking

This ensures future AI assistants have complete context for any additional work.

---

## 🎉 COMPLETION CELEBRATION

When all tasks are done:
- 🎯 **Code Quality**: From hacky to enterprise-grade
- 🔧 **Maintainability**: From nightmare to dream
- 📈 **Developer Experience**: From painful to pleasant
- ✅ **Native AOT**: Fully compatible and clean

**Ready to start? Copy/paste Task 1 prompt above!**
