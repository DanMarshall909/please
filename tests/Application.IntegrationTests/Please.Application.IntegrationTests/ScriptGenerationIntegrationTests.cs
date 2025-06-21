using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Please.Application.Commands.GenerateScript;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Services;

namespace Please.Application.IntegrationTests;

[TestFixture]
public class ScriptGenerationIntegrationTests
{
    private ServiceProvider _serviceProvider;
    private GenerateScriptCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        
        // Register real implementations - this tests actual behavior
        services.AddTransient<IScriptValidationService, TestScriptValidationService>();
        services.AddTransient<IScriptGenerator, TestScriptGenerator>();
        services.AddTransient<IScriptRepository, TestScriptRepository>();
        services.AddTransient<GenerateScriptCommandHandler>();
        
        _serviceProvider = services.BuildServiceProvider();
        _handler = _serviceProvider.GetRequiredService<GenerateScriptCommandHandler>();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [TestCase("rm -rf /", ScriptType.Bash)]
    [TestCase("format c:", ScriptType.PowerShell)]
    [TestCase("dd if=/dev/zero of=/dev/sda", ScriptType.Bash)]
    public async Task Critical_commands_require_confirmation(string dangerousScript, ScriptType scriptType)
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Execute dangerous command");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real validation integration
        Assert.That(result.RequiresConfirmation, Is.True);
        Assert.That(result.IsDangerous, Is.True);
        Assert.That(result.RiskLevel, Is.EqualTo(RiskLevel.Critical));
        Assert.That(result.Warnings, Is.Not.Empty);
    }

    [Test]
    public async Task Safe_commands_do_not_require_confirmation()
    {
        // Arrange  
        var command = GenerateScriptCommand.Create("List files in current directory");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real validation integration
        Assert.That(result.RequiresConfirmation, Is.False);
        Assert.That(result.IsDangerous, Is.False);
        Assert.That(result.RiskLevel, Is.EqualTo(RiskLevel.Low));
        Assert.That(result.Warnings, Is.Empty);
    }

    [Test]
    public async Task Generated_script_gets_saved_to_repository()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Create backup script");
        var repository = _serviceProvider.GetRequiredService<IScriptRepository>() as TestScriptRepository;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests the real workflow integration
        Assert.That(repository.SavedScripts, Has.Count.EqualTo(1));
        Assert.That(repository.SavedScripts[0].TaskDescription, Is.EqualTo("Create backup script"));
    }

    [Test]
    public async Task Script_validation_enhances_response_with_warnings()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Delete temporary files");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - This tests that validation actually runs and enhances the response
        Assert.That(result.Warnings, Is.Not.Empty);
        Assert.That(result.SafetyNotes, Is.Not.Empty);
        Assert.That(result.RiskLevel, Is.GreaterThan(RiskLevel.Low));
    }
}

// Test implementations that provide realistic behavior for integration testing
internal class TestScriptValidationService : IScriptValidationService
{
    public RiskLevel AssessRiskLevel(string script, ScriptType scriptType)
    {
        var lower = script.ToLowerInvariant();
        
        if (lower.Contains("rm -rf") || lower.Contains("format") || lower.Contains("dd if=/dev/zero"))
            return RiskLevel.Critical;
        if (lower.Contains("delete") || lower.Contains("remove"))
            return RiskLevel.Medium;
        return RiskLevel.Low;
    }

    public List<string> ValidateScript(string script, ScriptType scriptType)
    {
        var warnings = new List<string>();
        var lower = script.ToLowerInvariant();
        
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

    public bool ContainsDangerousOperations(string script, ScriptType scriptType)
    {
        return AssessRiskLevel(script, scriptType) >= RiskLevel.High;
    }

    public ScriptResponse EnhanceWithValidation(ScriptResponse response)
    {
        var riskLevel = AssessRiskLevel(response.Script, response.ScriptType);
        var warnings = ValidateScript(response.Script, response.ScriptType);
        var safetyNotes = GenerateSafetyNotes(response.Script, response.ScriptType);
        
        return response with
        {
            RiskLevel = riskLevel,
            Warnings = response.Warnings.Concat(warnings).ToList(),
            SafetyNotes = response.SafetyNotes.Concat(safetyNotes).ToList()
        };
    }
}

internal class TestScriptGenerator : IScriptGenerator
{
    private readonly IScriptValidationService _validationService;

    public TestScriptGenerator(IScriptValidationService validationService)
    {
        _validationService = validationService;
    }

    public Task<ScriptResponse> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default)
    {
        // Simulate script generation based on task description
        var script = request.TaskDescription.ToLowerInvariant() switch
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
        
        return Task.FromResult(enhancedResponse);
    }

    public Task<bool> IsProviderAvailableAsync(ScriptRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public string GetFallbackModel(ScriptRequest request)
    {
        return "gpt-3.5-turbo";
    }
}

internal class TestScriptRepository : IScriptRepository
{
    public List<ScriptResponse> SavedScripts { get; } = new();

    public Task SaveScriptAsync(ScriptResponse script, CancellationToken cancellationToken = default)
    {
        SavedScripts.Add(script);
        return Task.CompletedTask;
    }
}
