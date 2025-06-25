# Secure Configuration System Implementation Tasks

## 🎯 **Objective**
Enhance API key security by adding encrypted local storage while maintaining backward compatibility with current environment variable and User Secrets approach.

## 📊 **Project Scope**
- **Effort**: ~3.5 hours core functionality
- **Files**: 12 new files, 2 modified files
- **Tests**: 25+ unit tests, 5+ integration tests
- **Breaking Changes**: None (additive enhancement)

---

## 🏗️ **Architecture Overview**

```
Current: Env Vars → IConfiguration → Providers
Enhanced: Env Vars → Encrypted Storage → User Secrets → Interactive → Providers
```

**Priority Chain:**
1. Environment Variables (unchanged)
2. **NEW: Encrypted Local Storage** 
3. User Secrets/appsettings.json (unchanged)
4. **NEW: Interactive Prompt** (optional)

---

## 📋 **Task Breakdown**

### **Phase 1: Domain Layer Foundation** 
*Estimated: 30 minutes*

#### **Task 1.1: Create Core Security Interfaces**
- **File**: `src/Domain/Please.Domain/Interfaces/ISecureConfigurationService.cs`
- **Test Coverage**: Interface contracts only
- **Story**: As a developer, I need secure configuration interfaces so that I can implement encrypted key storage.

**Interface Requirements:**
```csharp
public interface ISecureConfigurationService
{
    Task<string?> GetApiKeyAsync(ProviderType provider);
    Task StoreApiKeyAsync(ProviderType provider, string apiKey);
    Task<bool> ValidateApiKeyAsync(ProviderType provider);
    Task<bool> HasValidApiKeyAsync(ProviderType provider);
    void ClearSensitiveData();
}
```

**Acceptance Criteria:**
- [ ] Interface compiles without errors
- [ ] All methods have XML documentation
- [ ] ProviderType enum supports all current providers
- [ ] Async patterns follow .NET conventions

#### **Task 1.2: Create Encrypted Storage Interface**
- **File**: `src/Domain/Please.Domain/Interfaces/IEncryptedStorage.cs`
- **Test Coverage**: Interface contracts only
- **Story**: As a developer, I need an encrypted storage abstraction so that I can store sensitive data securely across platforms.

**Interface Requirements:**
```csharp
public interface IEncryptedStorage
{
    Task<string?> GetEncryptedValueAsync(string key);
    Task StoreEncryptedValueAsync(string key, string value);
    Task<bool> ExistsAsync(string key);
    Task DeleteAsync(string key);
}
```

**Acceptance Criteria:**
- [ ] Interface supports async operations
- [ ] Key-value storage abstraction
- [ ] Null-safe return types
- [ ] Exception handling documented

### **Phase 2: Infrastructure Security Layer**
*Estimated: 90 minutes*

#### **Task 2.1: Implement Windows Encrypted Storage**
- **File**: `src/Infrastructure/Please.Infrastructure/Services/WindowsEncryptedStorage.cs`
- **Test File**: `tests/Infrastructure.UnitTests/Please.Infrastructure.UnitTests/Services/WindowsEncryptedStorageTests.cs`
- **Story**: As a Windows user, I need my API keys encrypted at rest so that other users cannot access my credentials.

**TDD Requirements:**
1. **RED**: Write failing test for key encryption/decryption
2. **GREEN**: Implement using Windows DPAPI (ProtectedData)
3. **REFACTOR**: Add error handling and logging

**Technical Specifications:**
- Use `System.Security.Cryptography.ProtectedData`
- Scope: `DataProtectionScope.CurrentUser`
- Storage Location: `%APPDATA%/Please/SecureKeys/`
- File Format: Base64 encoded encrypted data

**Unit Tests Required:**
```csharp
[Test] public async Task StoreEncryptedValue_encrypts_data_for_current_user()
[Test] public async Task GetEncryptedValue_decrypts_stored_data_correctly()
[Test] public async Task GetEncryptedValue_returns_null_when_key_not_found()
[Test] public async Task ExistsAsync_returns_true_when_key_exists()
[Test] public async Task DeleteAsync_removes_encrypted_key_file()
[Test] public async Task StoreEncryptedValue_overwrites_existing_key()
[Test] public async Task Different_users_cannot_decrypt_others_keys()
```

**Acceptance Criteria:**
- [ ] All unit tests pass with 95%+ coverage
- [ ] Keys encrypted with user-specific scope
- [ ] Graceful handling of missing directories
- [ ] Proper exception handling for crypto operations
- [ ] Logging for security events

#### **Task 2.2: Create Security Utilities**
- **File**: `src/Infrastructure/Please.Infrastructure/Security/SecureStringExtensions.cs`
- **Test File**: `tests/Infrastructure.UnitTests/Please.Infrastructure.UnitTests/Security/SecureStringExtensionsTests.cs`
- **Story**: As a developer, I need secure string utilities so that I can handle sensitive data safely in memory.

**TDD Requirements:**
1. **RED**: Write failing tests for secure string operations
2. **GREEN**: Implement extension methods
3. **REFACTOR**: Add memory clearing utilities

**Unit Tests Required:**
```csharp
[Test] public void ToSecureString_converts_string_to_secure_string()
[Test] public void ToUnsecuredString_converts_secure_string_back()
[Test] public void ClearString_overwrites_string_in_memory()
[Test] public void SecureString_is_disposed_properly()
```

**Acceptance Criteria:**
- [ ] All tests pass with 90%+ coverage
- [ ] Memory cleared after sensitive operations
- [ ] Proper disposal of SecureString objects
- [ ] Thread-safe operations

#### **Task 2.3: Implement API Key Validator**
- **File**: `src/Infrastructure/Please.Infrastructure/Security/ApiKeyValidator.cs`
- **Test File**: `tests/Infrastructure.UnitTests/Please.Infrastructure.UnitTests/Security/ApiKeyValidatorTests.cs`
- **Story**: As a user, I need my API keys validated so that I know immediately if they're invalid.

**TDD Requirements:**
1. **RED**: Write failing tests for each provider validation
2. **GREEN**: Implement minimal API calls for validation
3. **REFACTOR**: Add timeout and error handling

**Unit Tests Required:**
```csharp
[Test] public async Task ValidateOpenAiKey_returns_true_for_valid_key()
[Test] public async Task ValidateOpenAiKey_returns_false_for_invalid_key()
[Test] public async Task ValidateAnthropicKey_handles_timeout_gracefully()
[Test] public async Task ValidateGeminiKey_handles_network_errors()
[Test] public async Task ValidateOpenRouterKey_validates_correctly()
```

**Acceptance Criteria:**
- [ ] All tests pass (using mocked HTTP responses)
- [ ] 30-second timeout for validation calls
- [ ] Proper error handling for network issues
- [ ] Minimal API calls (models list or simple completion)
- [ ] Support for all current providers

### **Phase 3: Core Secure Configuration Service**
*Estimated: 75 minutes*

#### **Task 3.1: Implement SecureConfigurationService**
- **File**: `src/Infrastructure/Please.Infrastructure/Services/SecureConfigurationService.cs`
- **Test File**: `tests/Infrastructure.UnitTests/Please.Infrastructure.UnitTests/Services/SecureConfigurationServiceTests.cs`
- **Story**: As a user, I need a secure configuration service so that my API keys are handled with maximum security while maintaining ease of use.

**TDD Requirements:**
1. **RED**: Write failing tests for priority chain logic
2. **GREEN**: Implement configuration resolution chain
3. **REFACTOR**: Add memory management and caching

**Priority Chain Implementation:**
```csharp
public async Task<string?> GetApiKeyAsync(ProviderType provider)
{
    // 1. Environment Variables (highest priority)
    var envKey = GetEnvironmentVariable(provider);
    if (!string.IsNullOrEmpty(envKey)) return envKey;
    
    // 2. Encrypted Local Storage
    var encryptedKey = await _encryptedStorage.GetEncryptedValueAsync(GetKeyName(provider));
    if (!string.IsNullOrEmpty(encryptedKey)) return encryptedKey;
    
    // 3. User Secrets/Configuration
    var configKey = _configuration[GetConfigKeyName(provider)];
    if (!string.IsNullOrEmpty(configKey)) return configKey;
    
    // 4. Interactive Prompt (if enabled)
    if (_allowInteractivePrompt)
        return await PromptForKeyAsync(provider);
        
    return null;
}
```

**Unit Tests Required:**
```csharp
[Test] public async Task GetApiKey_environment_variable_takes_highest_priority()
[Test] public async Task GetApiKey_falls_back_to_encrypted_storage_when_env_empty()
[Test] public async Task GetApiKey_falls_back_to_configuration_when_storage_empty()
[Test] public async Task GetApiKey_prompts_interactively_when_all_sources_empty()
[Test] public async Task GetApiKey_returns_null_when_no_interactive_and_no_keys()
[Test] public async Task StoreApiKey_encrypts_and_stores_key_properly()
[Test] public async Task ValidateApiKey_calls_validator_service()
[Test] public async Task HasValidApiKey_returns_true_when_key_exists_and_valid()
[Test] public async Task ClearSensitiveData_removes_cached_keys_from_memory()
[Test] public async Task Multiple_concurrent_calls_dont_cause_race_conditions()
```

**Acceptance Criteria:**
- [ ] All unit tests pass with 95%+ coverage
- [ ] Priority chain works correctly
- [ ] Thread-safe implementation
- [ ] Memory is cleared after operations
- [ ] Proper exception handling and logging
- [ ] Caching prevents repeated decryption

### **Phase 4: Dependency Injection Integration**
*Estimated: 30 minutes*

#### **Task 4.1: Update DI Registration**
- **File**: `src/Infrastructure/Please.Infrastructure/DependencyInjection.cs` (modify existing)
- **Test File**: `tests/Infrastructure.UnitTests/Please.Infrastructure.UnitTests/DependencyInjectionTests.cs`
- **Story**: As a developer, I need the secure configuration services registered so that they're available throughout the application.

**TDD Requirements:**
1. **RED**: Write failing tests for service registration
2. **GREEN**: Add new service registrations
3. **REFACTOR**: Update ProviderConfiguration to use secure service

**New Registrations:**
```csharp
// Add security services
services.AddSingleton<IEncryptedStorage, WindowsEncryptedStorage>();
services.AddSingleton<ISecureConfigurationService, SecureConfigurationService>();
services.AddSingleton<ApiKeyValidator>();

// Enhanced provider configuration
services.AddSingleton<ProviderConfiguration>(async provider =>
{
    var secureConfig = provider.GetRequiredService<ISecureConfigurationService>();
    return new ProviderConfiguration
    {
        OpenAi = new OpenAiConfiguration
        {
            ApiKey = await secureConfig.GetApiKeyAsync(ProviderType.OpenAI) ?? "",
            // ... rest unchanged
        }
        // ... other providers
    };
});
```

**Unit Tests Required:**
```csharp
[Test] public void AddInfrastructure_registers_secure_configuration_service()
[Test] public void AddInfrastructure_registers_encrypted_storage_service()
[Test] public void AddInfrastructure_registers_api_key_validator()
[Test] public void ProviderConfiguration_uses_secure_configuration_service()
[Test] public void All_required_services_can_be_resolved()
```

**Acceptance Criteria:**
- [ ] All existing tests continue to pass
- [ ] New services are properly registered
- [ ] ProviderConfiguration uses SecureConfigurationService
- [ ] No breaking changes to existing registrations
- [ ] Dependency injection container resolves all services

### **Phase 5: Integration Testing**
*Estimated: 45 minutes*

#### **Task 5.1: Create Integration Tests**
- **File**: `tests/Application.IntegrationTests/Please.Application.IntegrationTests/SecureConfigurationIntegrationTests.cs`
- **Story**: As a developer, I need integration tests so that I can verify the secure configuration system works end-to-end.

**Integration Test Scenarios:**
```csharp
[Test] public async Task End_to_end_key_storage_and_retrieval_works()
[Test] public async Task Environment_variable_overrides_stored_key()
[Test] public async Task Provider_factory_uses_securely_stored_keys()
[Test] public async Task Key_validation_works_with_real_api_endpoints()
[Test] public async Task Multiple_providers_can_store_and_retrieve_keys()
[Test] public async Task Configuration_priority_chain_works_correctly()
```

**Acceptance Criteria:**
- [ ] All integration tests pass
- [ ] Tests cover end-to-end scenarios
- [ ] Real file system operations work
- [ ] Provider integration works correctly
- [ ] Performance is acceptable (< 1 second per operation)

### **Phase 6: Optional Management Commands**
*Estimated: 45 minutes (Optional)*

#### **Task 6.1: Create Configuration Management Commands**
- **File**: `src/Presentation/Please.Console/Commands/ConfigCommand.cs`
- **Test File**: `tests/Presentation.UnitTests/Please.Presentation.UnitTests/Commands/ConfigCommandTests.cs`
- **Story**: As a power user, I need command-line tools to manage my API keys so that I can configure the application without editing files.

**Command Interface:**
```bash
please config set-key openai sk-...          # Store encrypted key
please config test-key openai                # Validate key
please config list-keys                      # Show configured providers
please config clear-keys                     # Remove all stored keys
please config rotate-key openai              # Prompt for new key
```

**Unit Tests Required:**
```csharp
[Test] public async Task SetKey_command_stores_key_securely()
[Test] public async Task TestKey_command_validates_key_and_reports_status()
[Test] public async Task ListKeys_command_shows_configured_providers()
[Test] public async Task ClearKeys_command_removes_all_stored_keys()
[Test] public async Task RotateKey_command_prompts_for_new_key()
```

**Acceptance Criteria:**
- [ ] All command tests pass
- [ ] Commands integrate with main application
- [ ] Secure input handling for API keys
- [ ] Clear success/error messages
- [ ] Help documentation for each command

---

## ✅ **Definition of Done**

### **Code Quality Gates**
- [ ] All unit tests pass with 90%+ coverage
- [ ] All integration tests pass
- [ ] No breaking changes to existing functionality
- [ ] Code follows project conventions and patterns
- [ ] All security best practices implemented
- [ ] XML documentation on public APIs

### **Security Verification**
- [ ] API keys encrypted at rest using Windows DPAPI
- [ ] Memory cleared after sensitive operations
- [ ] Priority chain respects security hierarchy
- [ ] User-scoped encryption (other users can't decrypt)
- [ ] Audit logging for security events
- [ ] Input validation on all user-provided data

### **User Experience**
- [ ] Backward compatibility maintained
- [ ] Clear error messages for invalid keys
- [ ] Performance acceptable (< 1 second for key operations)
- [ ] No additional setup required for existing users
- [ ] Optional interactive prompts work correctly

### **Documentation**
- [ ] Architecture documentation updated
- [ ] Configuration guide updated with new options
- [ ] Security best practices documented
- [ ] Troubleshooting guide for common issues
- [ ] API documentation for new interfaces

---

## 🚨 **Risks and Mitigations**

### **Technical Risks**
1. **Risk**: Windows DPAPI might fail on some systems
   - **Mitigation**: Graceful fallback to existing configuration methods
   - **Detection**: Integration tests on different Windows versions

2. **Risk**: Performance impact from encryption/decryption
   - **Mitigation**: Caching and lazy loading
   - **Detection**: Performance benchmarks in tests

3. **Risk**: Memory leaks from SecureString usage
   - **Mitigation**: Proper disposal patterns and automated testing
   - **Detection**: Memory profiling during integration tests

### **User Experience Risks**
1. **Risk**: Breaking changes for existing users
   - **Mitigation**: Additive changes only, maintain full backward compatibility
   - **Detection**: Run existing integration tests against new code

2. **Risk**: Confusing error messages
   - **Mitigation**: Clear, actionable error messages with suggestions
   - **Detection**: User acceptance testing scenarios

### **Security Risks**
1. **Risk**: Keys could be exposed during debugging
   - **Mitigation**: Secure string handling and memory clearing
   - **Detection**: Security code review and penetration testing

2. **Risk**: Encryption keys could be compromised
   - **Mitigation**: User-scoped Windows DPAPI, rotation capabilities
   - **Detection**: Security audit of encryption implementation

---

## 📦 **Dependencies**

### **External Dependencies**
- `System.Security.Cryptography` (already available in .NET 8)
- `System.Security` for SecureString (already available)
- No new NuGet packages required

### **Internal Dependencies**
- Existing ProviderConfiguration classes
- Existing ProviderType enum
- IConfiguration infrastructure
- Existing test utilities

### **Environment Dependencies**
- Windows DPAPI (Windows-specific implementation)
- File system access for encrypted storage
- Environment variable access (existing)

---

## 🎯 **Success Metrics**

### **Quantitative Goals**
- [ ] **Security**: 100% of stored API keys encrypted at rest
- [ ] **Performance**: Key operations complete in < 1 second
- [ ] **Reliability**: 99%+ success rate for key storage/retrieval
- [ ] **Test Coverage**: 90%+ code coverage on new components
- [ ] **Backward Compatibility**: 100% of existing functionality preserved

### **Qualitative Goals**
- [ ] **User Experience**: No additional setup required for existing users
- [ ] **Security Posture**: Significantly improved protection of sensitive data
- [ ] **Developer Experience**: Clean, testable, maintainable code
- [ ] **Documentation**: Clear guidance for configuration and troubleshooting
- [ ] **Future Readiness**: Architecture supports Linux/Mac implementations

---

## 📅 **Implementation Schedule**

### **Sprint 1: Foundation (Day 1)**
- Task 1.1: Core Security Interfaces (30 min)
- Task 1.2: Encrypted Storage Interface (15 min)
- Task 2.1: Windows Encrypted Storage (60 min)

### **Sprint 2: Core Services (Day 2)**
- Task 2.2: Security Utilities (45 min)
- Task 2.3: API Key Validator (45 min)
- Task 3.1: SecureConfigurationService (75 min)

### **Sprint 3: Integration (Day 3)**
- Task 4.1: DI Registration Updates (30 min)
- Task 5.1: Integration Tests (45 min)
- Task 6.1: Management Commands (Optional, 45 min)

**Total Estimated Effort: 3.5 hours core + 0.75 hours optional = 4.25 hours**

---

This task breakdown provides a complete roadmap for implementing the secure configuration system with proper TDD practices, comprehensive testing, and zero breaking changes to existing functionality.
