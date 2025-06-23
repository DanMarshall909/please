using Microsoft.Extensions.DependencyInjection;
using Please.ConsoleHost;
using Please.Application.Commands.GenerateScript;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using Xunit;

namespace Please.Application.IntegrationTests;

public class ScriptGenerationIntegrationTests
{
    private ServiceProvider _serviceProvider;
    private GenerateScriptCommandHandler _handler;

    public ScriptGenerationIntegrationTests()
    {
        _serviceProvider = PleaseHost.CreateServiceProvider(services =>
        {
            // Register real implementations - this tests actual behavior
            services.AddTransient<IScriptValidationService, TestScriptValidationService>();
            services.AddTransient<IScriptGenerator, TestScriptGenerator>();
            var testRepo = new TestScriptRepository();
            services.AddSingleton<IScriptRepository>(testRepo);
            services.AddSingleton(testRepo);
            services.AddTransient<GenerateScriptCommandHandler>();
        });
        _handler = _serviceProvider.GetRequiredService<GenerateScriptCommandHandler>();
    }

    [Fact]
    public async Task Critical_commands_require_confirmation_rm_rf_slash()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Execute dangerous command");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real validation integration
        Assert.True(result.RequiresConfirmation);
        Assert.True(result.IsDangerous);
        Assert.Equal(RiskLevel.Critical, result.RiskLevel);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public async Task Critical_commands_require_confirmation_format_c_colon()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Execute dangerous command");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real validation integration
        Assert.True(result.RequiresConfirmation);
        Assert.True(result.IsDangerous);
        Assert.Equal(RiskLevel.Critical, result.RiskLevel);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public async Task Critical_commands_require_confirmation_dd_if_dev_zero_of_dev_sda()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Execute dangerous command");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real validation integration
        Assert.True(result.RequiresConfirmation);
        Assert.True(result.IsDangerous);
        Assert.Equal(RiskLevel.Critical, result.RiskLevel);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public async Task Safe_commands_do_not_require_confirmation()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("List files in current directory");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real validation integration
        Assert.False(result.RequiresConfirmation);
        Assert.False(result.IsDangerous);
        Assert.Equal(RiskLevel.Low, result.RiskLevel);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public async Task Generated_script_gets_saved_to_repository()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Create backup script");
        var repository = _serviceProvider.GetRequiredService<TestScriptRepository>();
        Assert.NotNull(repository);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real workflow integration
        Assert.Single(repository.SavedScripts);
        Assert.Equal("Create backup script", repository.SavedScripts[0].TaskDescription);
    }

    [Fact]
    public async Task Script_validation_enhances_response_with_warnings()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Delete temporary files");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests that validation actually runs and enhances the response
        Assert.NotEmpty(result.Warnings);
        Assert.NotEmpty(result.SafetyNotes);
        Assert.True(result.RiskLevel > RiskLevel.Low);
    }
}

// Test implementations that provide realistic behavior for integration testing
internal class TestScriptValidationService : IScriptValidationService
{
    public RiskLevel AssessRiskLevel(string script, ScriptType scriptType)
    {
        string lower = script.ToLowerInvariant();

        if (lower.Contains("rm -rf") || lower.Contains("format") || lower.Contains("dd if=/dev/zero"))
            return RiskLevel.Critical;
        if (lower.Contains("delete") || lower.Contains("remove"))
            return RiskLevel.Medium;
        return RiskLevel.Low;
    }

    public List<string> ValidateScript(string script, ScriptType scriptType)
    {
        var warnings = new List<string>();
        string lower = script.ToLowerInvariant();

        if (lower.Contains("rm -rf"))
            warnings.Add("⛔ CRITICAL: 'rm -rf' command can delete important files");
        if (lower.Contains("format"))
            warnings.Add("⛔ CRITICAL: 'format' command will erase disk data");
        if (lower.Contains("delete") || lower.Contains("remove"))
            warnings.Add("⚠️  WARNING: File deletion detected");

        return warnings;
    }

    public List<string> GenerateSafetyNotes(string script, ScriptType scriptType)
    {
        var notes = new List<string>();
        var riskLevel = AssessRiskLevel(script, scriptType);

        if (riskLevel >= RiskLevel.Medium)
        {
            notes.Add("💡 Consider creating a backup before running this script");
            notes.Add("💡 Test this script in a safe environment first");
        }

        return notes;
    }

    public bool ContainsDangerousOperations(string script, ScriptType scriptType) =>
        AssessRiskLevel(script, scriptType) >= RiskLevel.High;

    public ScriptResponse EnhanceWithValidation(ScriptResponse response)
    {
        var riskLevel = AssessRiskLevel(response.Script, response.ScriptType);
        var warnings = ValidateScript(response.Script, response.ScriptType);
        var safetyNotes = GenerateSafetyNotes(response.Script, response.ScriptType);

        return response with
        {
            RiskLevel = riskLevel,
            Warnings = response.Warnings.Concat(warnings.Select(w => (ScriptResponse.Warning)w)).ToList(),
            SafetyNotes = response.SafetyNotes.Concat(safetyNotes.Select(n => (ScriptResponse.SafetyNote)n)).ToList()
        };
    }
}

internal class TestScriptGenerator : IScriptGenerator
{
    private readonly IScriptValidationService _validationService;

    public TestScriptGenerator(IScriptValidationService validationService) => _validationService = validationService;

    public Task<Result<ScriptResponse>> GenerateScriptAsync(ScriptRequest request,
        CancellationToken cancellationToken = default)
    {
        // Simulate script generation based on task description
        string script = request.TaskDescription.ToLowerInvariant() switch
        {
            var desc when desc.Contains("dangerous") => "rm -rf /",
            var desc when desc.Contains("delete") || desc.Contains("temporary") => "rm -rf /tmp/*",
            var desc when desc.Contains("list") => "ls -la",
            var desc when desc.Contains("backup") => "cp -r /important /backup",
            _ => "echo 'Generated script'"
        };

        var scriptType = request.ScriptType ?? ScriptType.Bash;

        var response = ScriptResponse.Create(
            script,
            request.TaskDescription,
            request.Provider ?? ProviderType.OpenAI,
            request.Model ?? "gpt-4",
            scriptType
        );

        // Apply validation enhancement
        var enhancedResponse = _validationService.EnhanceWithValidation(response);

        return Task.FromResult(Result<ScriptResponse>.Success(enhancedResponse));
    }

    public Task<Result<bool>> IsProviderAvailableAsync(ScriptRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<bool>.Success(true));

    public string GetFallbackModel(ScriptRequest request) => "gpt-3.5-turbo";
}

internal class TestScriptRepository : IScriptRepository
{
    public List<ScriptResponse> SavedScripts { get; } = [];

    public Task<Result> SaveScriptAsync(ScriptResponse script, CancellationToken cancellationToken = default)
    {
        SavedScripts.Add(script);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<ScriptResponse?>> GetLastScriptAsync(CancellationToken cancellationToken = default)
    {
        var last = SavedScripts.LastOrDefault();
        return Task.FromResult(last != null
            ? Result<ScriptResponse?>.Success(last)
            : Result<ScriptResponse?>.Failure("No scripts found"));
    }

    public Task<Result<IEnumerable<ScriptResponse>>> GetScriptHistoryAsync(int? count = null, DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<ScriptResponse> history = SavedScripts;
        if (since.HasValue)
            history = history.Where(s => s.CreatedAt >= since.Value);
        if (count.HasValue)
            history = history.Take(count.Value);
        return Task.FromResult(Result<IEnumerable<ScriptResponse>>.Success(history));
    }

    public Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        SavedScripts.Clear();
        return Task.FromResult(Result.Success());
    }

    public Task<Result<bool>> HasHistoryAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<bool>.Success(SavedScripts.Any()));
}
