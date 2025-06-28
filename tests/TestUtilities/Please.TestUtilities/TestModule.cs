using Please.Domain.Interfaces;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Common;

namespace Please.TestUtilities;

using Microsoft.Extensions.DependencyInjection;

public static class TestModule
{
    public static IServiceCollection AddTestDoubles(this IServiceCollection services)
    {
        services.AddSingleton<FakeScriptGenerator>();
        services.AddSingleton<FakeScriptRepository>();
        services.AddSingleton<FakeContextService>();
        services.AddSingleton<FakeConsoleUIService>();
        services.AddSingleton<FakeClipboardService>();
        services.AddSingleton<FakeFileService>();

        services.AddSingleton<IScriptGenerator>(sp => sp.GetRequiredService<FakeScriptGenerator>());
        services.AddSingleton<IScriptRepository>(sp => sp.GetRequiredService<FakeScriptRepository>());
        services.AddSingleton<IContextService>(sp => sp.GetRequiredService<FakeContextService>());
        services.AddSingleton<IConsoleUIService>(sp => sp.GetRequiredService<FakeConsoleUIService>());
        services.AddSingleton<IClipboardService>(sp => sp.GetRequiredService<FakeClipboardService>());
        services.AddSingleton<IFileService>(sp => sp.GetRequiredService<FakeFileService>());

        return services;
    }

    public class FakeConsoleUIService : IConsoleUIService
    {
        public void DisplayScript(string script, string title) { }
        public Task DisplayProgressAsync(string message, Func<Task> action) => action();
        public Task<T> DisplayProgressAsync<T>(string message, Func<Task<T>> action) => action();
        public int DisplayInteractiveMenu(string[] options) => 0; // Default to first option
        public void DisplayRiskWarning(string riskLevel, string[] warnings) { }
        public void DisplayBanner(string version, string description) { }
        public void DisplayScriptWithSyntaxHighlighting(string script, string title, ScriptType scriptType) { }
        public void DisplayScriptResponse(ScriptResponse response) { }
        public void DisplaySafetyNotes(IEnumerable<string> safetyNotes) { }
        public Task DisplayEnhancedProgressAsync(string title, string[] steps, Func<string, int, Task> stepAction)
        {
            return Task.CompletedTask;
        }
        public void DisplayScriptPreview(ScriptResponse response) { }
        public Task<string?> EditScriptExternallyAsync(string script, ScriptType scriptType, string taskDescription)
        {
            return Task.FromResult<string?>(script); // Return unchanged
        }
        public bool ConfirmScriptExecution(ScriptResponse response) => false; // Default to not execute for safety
    }

    public class FakeClipboardService : IClipboardService
    {
        public bool IsSupported() => true;
        public Task<bool> SetTextAsync(string text) => Task.FromResult(true);
        public Task<string?> GetTextAsync() => Task.FromResult<string?>("test clipboard content");
    }

    public class FakeFileService : IFileService
    {
        public Task<Result<string>> SaveScriptToFileAsync(ScriptResponse script, string? directory = null, string? fileName = null)
        {
            var testPath = Path.Combine(Path.GetTempPath(), GenerateFileName(script.TaskDescription) + GetFileExtension(script.ScriptType));
            return Task.FromResult(Result<string>.Success(testPath));
        }

        public string GetFileExtension(ScriptType scriptType)
        {
            return scriptType switch
            {
                ScriptType.PowerShell => ".ps1",
                ScriptType.Bash => ".sh",
                ScriptType.Command => ".bat",
                ScriptType.Python => ".py",
                _ => ".txt"
            };
        }

        public string GenerateFileName(string taskDescription)
        {
            return string.IsNullOrWhiteSpace(taskDescription) ? "test_script" : "test_script";
        }

        public string GetDefaultSaveDirectory()
        {
            return Path.GetTempPath();
        }
    }
}
