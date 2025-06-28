using Microsoft.Extensions.Logging;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using System.Text;
using System.Text.RegularExpressions;

namespace Please.Infrastructure.Services;

/// <summary>
/// Cross-platform file service implementation
/// </summary>
public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly IPlatformService _platformService;

    public FileService(ILogger<FileService> logger, IPlatformService platformService)
    {
        _logger = logger;
        _platformService = platformService;
    }

    public async Task<Result<string>> SaveScriptToFileAsync(ScriptResponse script, string? directory = null, string? fileName = null)
    {
        try
        {
            if (script == null)
                return Result<string>.Failure("Script cannot be null");

            if (string.IsNullOrWhiteSpace(script.Script))
                return Result<string>.Failure("Script content cannot be empty");

            // Use provided directory or default
            var saveDirectory = directory ?? GetDefaultSaveDirectory();
            
            // Ensure directory exists
            Directory.CreateDirectory(saveDirectory);

            // Generate file name if not provided
            var baseName = fileName ?? GenerateFileName(script.TaskDescription);
            var extension = GetFileExtension(script.ScriptType);
            var fullFileName = baseName + extension;

            // Handle file name conflicts
            var fullPath = Path.Combine(saveDirectory, fullFileName);
            var counter = 1;
            while (File.Exists(fullPath))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fullFileName);
                fullPath = Path.Combine(saveDirectory, $"{nameWithoutExt}_{counter}{extension}");
                counter++;
            }

            // Prepare file content with metadata header
            var content = BuildFileContent(script);

            // Write file
            await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);

            _logger.LogInformation("Script saved to file: {FilePath}", fullPath);
            return Result<string>.Success(fullPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when saving script to file");
            return Result<string>.Failure($"Access denied: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError(ex, "Directory not found when saving script");
            return Result<string>.Failure($"Directory not found: {ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error when saving script to file");
            return Result<string>.Failure($"File operation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when saving script to file");
            return Result<string>.Failure($"Failed to save script: {ex.Message}");
        }
    }

    public string GetFileExtension(ScriptType scriptType)
    {
        return scriptType switch
        {
            ScriptType.PowerShell => ".ps1",
            ScriptType.Bash => ".sh",
            ScriptType.Command => OperatingSystem.IsWindows() ? ".bat" : ".sh",
            ScriptType.Python => ".py",
            ScriptType.Auto => OperatingSystem.IsWindows() ? ".ps1" : ".sh",
            _ => ".txt"
        };
    }

    public string GenerateFileName(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return $"please_script_{DateTime.Now:yyyyMMdd_HHmmss}";

        // Clean the task description to make it file-safe
        var fileName = taskDescription.Trim();
        
        // Replace common words and phrases with shorter versions
        fileName = fileName.Replace("create a script", "script", StringComparison.OrdinalIgnoreCase);
        fileName = fileName.Replace("generate a script", "script", StringComparison.OrdinalIgnoreCase);
        fileName = fileName.Replace("write a script", "script", StringComparison.OrdinalIgnoreCase);
        fileName = fileName.Replace("script to", "", StringComparison.OrdinalIgnoreCase);
        fileName = fileName.Replace("script that", "", StringComparison.OrdinalIgnoreCase);
        
        // Remove invalid file name characters
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var invalidChar in invalidChars)
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        // Replace spaces and special characters
        fileName = Regex.Replace(fileName, @"[^\w\-_]", "_");
        
        // Remove multiple underscores
        fileName = Regex.Replace(fileName, @"_+", "_");
        
        // Trim underscores from start and end
        fileName = fileName.Trim('_');
        
        // Limit length
        if (fileName.Length > 50)
        {
            fileName = fileName.Substring(0, 50).TrimEnd('_');
        }
        
        // Ensure we have a valid name
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"please_script_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        return fileName.ToLowerInvariant();
    }

    public string GetDefaultSaveDirectory()
    {
        try
        {
            // Try to use Documents folder first
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrEmpty(documentsPath) && Directory.Exists(documentsPath))
            {
                var pleaseScriptsPath = Path.Combine(documentsPath, "Please Scripts");
                return pleaseScriptsPath;
            }

            // Fall back to user's home directory
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(homePath) && Directory.Exists(homePath))
            {
                return Path.Combine(homePath, "Please Scripts");
            }

            // Last resort: current directory
            return Path.Combine(Directory.GetCurrentDirectory(), "scripts");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not determine default save directory, using current directory");
            return Path.Combine(Directory.GetCurrentDirectory(), "scripts");
        }
    }

    private static string BuildFileContent(ScriptResponse script)
    {
        var sb = new StringBuilder();
        
        // Add header comment based on script type
        var commentPrefix = script.ScriptType switch
        {
            ScriptType.PowerShell => "#",
            ScriptType.Python => "#",
            ScriptType.Bash => "#",
            ScriptType.Command => "REM",
            _ => "#"
        };

        sb.AppendLine($"{commentPrefix} Generated by Please v6.0.0");
        sb.AppendLine($"{commentPrefix} Task: {script.TaskDescription}");
        sb.AppendLine($"{commentPrefix} Provider: {script.Provider} ({script.Model})");
        sb.AppendLine($"{commentPrefix} Generated: {script.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"{commentPrefix} Risk Level: {script.RiskLevel}");
        
        if (script.Warnings.Any())
        {
            sb.AppendLine($"{commentPrefix}");
            sb.AppendLine($"{commentPrefix} Warnings:");
            foreach (var warning in script.Warnings)
            {
                sb.AppendLine($"{commentPrefix}   - {warning.Message}");
            }
        }

        if (script.SafetyNotes.Any())
        {
            sb.AppendLine($"{commentPrefix}");
            sb.AppendLine($"{commentPrefix} Safety Notes:");
            foreach (var note in script.SafetyNotes)
            {
                sb.AppendLine($"{commentPrefix}   - {note.Message}");
            }
        }

        sb.AppendLine($"{commentPrefix}");
        sb.AppendLine();
        
        // Add the actual script content
        sb.AppendLine(script.Script);

        return sb.ToString();
    }
}