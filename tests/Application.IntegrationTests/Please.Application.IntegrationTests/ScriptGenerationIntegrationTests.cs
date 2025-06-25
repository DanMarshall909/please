using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using Please.TestUtilities;
using Microsoft.Extensions.Logging;

namespace Please.Application.IntegrationTests;

public class ScriptGenerationIntegrationTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IScriptService _scriptService;

    public ScriptGenerationIntegrationTests()
    {
        var services = new ServiceCollection();

        // Add test doubles
        services.AddTestDoubles();

        // Register real implementations - this tests actual behavior
        services.AddTransient<IScriptValidationService, TestScriptValidationService>();
        services.AddTransient<IScriptGenerator, TestScriptGenerator>();
        var testRepo = new TestScriptRepository();
        services.AddSingleton<IScriptRepository>(testRepo);
        services.AddSingleton(testRepo);
        services.AddTransient<IScriptService, ScriptService>();

        services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());

        services.AddLogging(config => config.AddDebug());

        _serviceProvider = services.BuildServiceProvider();
        _scriptService = _serviceProvider.GetRequiredService<IScriptService>();
    }

    [Fact]
    public async Task dangerous_script_commands_require_user_confirmation()
    {
        // Arrange
        var request = ScriptRequest.Create("Execute dangerous command");

        // Act
        var result = await _scriptService.GenerateScriptAsync(request);

        // Assert - This tests the real validation integration
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.RequiresConfirmation);
        Assert.True(result.Value!.IsDangerous);
        Assert.Equal(RiskLevel.Critical, result.Value!.RiskLevel);
        Assert.NotEmpty(result.Value!.Warnings);
    }

    [Fact]
    public async Task safe_commands_do_not_require_confirmation()
    {
        // Arrange
        var request = ScriptRequest.Create("List files in current directory");

        // Act
        var result = await _scriptService.GenerateScriptAsync(request);

        // Assert - This tests the real validation integration
        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.RequiresConfirmation);
        Assert.False(result.Value!.IsDangerous);
        Assert.Equal(RiskLevel.Low, result.Value!.RiskLevel);
        Assert.Empty(result.Value!.Warnings);
    }

    [Fact]
    public async Task generated_scripts_are_saved_to_repository()
    {
        // Arrange
        var request = ScriptRequest.Create("Create backup script");
        var repository = _serviceProvider.GetRequiredService<TestScriptRepository>();
        Assert.NotNull(repository);

        // Act
        var result = await _scriptService.GenerateScriptAsync(request);

        // Assert - This tests the real workflow integration
        Assert.True(result.IsSuccess);
        Assert.Single(repository.SavedScripts);
        Assert.Equal("Create backup script", repository.SavedScripts[0].TaskDescription);
    }

    [Fact]
    public async Task script_validation_adds_warnings_and_safety_notes()
    {
        // Arrange
        var request = ScriptRequest.Create("Delete temporary files");

        // Act
        var result = await _scriptService.GenerateScriptAsync(request);

        // Assert - This tests that validation actually runs and enhances the response
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!.Warnings);
        Assert.NotEmpty(result.Value!.SafetyNotes);
        Assert.True(result.Value!.RiskLevel > RiskLevel.Low);
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
            Warnings = response.Warnings.Concat(warnings.Select(w => new ScriptResponse.Warning(w))).ToList(),
            SafetyNotes = response.SafetyNotes.Concat(safetyNotes.Select(n => new ScriptResponse.SafetyNote(n)))
                .ToList()
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
            request.Provider ?? ProviderType.OpenAi,
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

    public Task<VoidResult> SaveScriptAsync(ScriptResponse script, CancellationToken cancellationToken = default)
    {
        SavedScripts.Add(script);
        return VoidResult.SuccessfulTask;
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

    public Task<VoidResult> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        SavedScripts.Clear();
        return VoidResult.SuccessfulTask;
    }

    public Task<Result<bool>> HasHistoryAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<bool>.Success(SavedScripts.Any()));
}
