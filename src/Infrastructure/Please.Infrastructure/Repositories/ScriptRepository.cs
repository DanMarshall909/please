using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Interfaces;
using Please.Domain.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Please.Infrastructure.Repositories;

[JsonSerializable(typeof(List<ScriptResponse>))]
[JsonSerializable(typeof(ScriptResponse))]
internal partial class ScriptRepositoryJsonContext : JsonSerializerContext
{
}

/// <summary>
/// File-based implementation of script repository using platform-appropriate directories
/// </summary>
public class ScriptRepository : IScriptRepository
{
    private readonly IPlatformService _platformService;
    private readonly string _historyFilePath;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = ScriptRepositoryJsonContext.Default
    };

    public ScriptRepository(IPlatformService platformService)
    {
        _platformService = platformService;
        var dataDir = _platformService.GetDataDirectory();
        Directory.CreateDirectory(dataDir);
        _historyFilePath = Path.Combine(dataDir, "script-history.json");
    }

    public Task<VoidResult> SaveScriptAsync(ScriptResponse response, CancellationToken cancellationToken = default)
    {
        if (response == null) return Task.FromResult(VoidResult.Failure("Script response cannot be null"));

        try
        {
            lock (_lock)
            {
                var scripts = LoadScriptsFromFile();
                scripts.Add(response);
                SaveScriptsToFile(scripts);
            }

            return Task.FromResult(VoidResult.Success);
        }
        catch (Exception ex)
        {
            return Task.FromResult(VoidResult.Failure($"Failed to save script: {ex.Message}"));
        }
    }

    public Task<Result<ScriptResponse?>> GetLastScriptAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                var scripts = LoadScriptsFromFile();
                var lastScript = scripts
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefault();

                return Task.FromResult(Result<ScriptResponse?>.Success(lastScript));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ScriptResponse?>.Failure($"Failed to load script history: {ex.Message}"));
        }
    }

    public Task<Result<IEnumerable<ScriptResponse>>> GetScriptHistoryAsync(
        int? count = null,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                var scripts = LoadScriptsFromFile();
                var query = scripts.AsEnumerable();

                // Filter by date if specified
                if (since.HasValue) query = query.Where(s => s.CreatedAt >= since.Value);

                // Order by most recent first
                query = query.OrderByDescending(s => s.CreatedAt);

                // Apply count limit if specified
                if (count.HasValue && count.Value > 0) query = query.Take(count.Value);

                var results = query.ToList();
                return Task.FromResult(Result<IEnumerable<ScriptResponse>>.Success(results));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<IEnumerable<ScriptResponse>>.Failure($"Failed to load script history: {ex.Message}"));
        }
    }

    public Task<VoidResult> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                SaveScriptsToFile(new List<ScriptResponse>());
            }

            return Task.FromResult(VoidResult.Success);
        }
        catch (Exception ex)
        {
            return Task.FromResult(VoidResult.Failure($"Failed to clear script history: {ex.Message}"));
        }
    }

    public Task<Result<bool>> HasHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lock)
            {
                var scripts = LoadScriptsFromFile();
                bool hasHistory = scripts.Count > 0;
                return Task.FromResult(Result<bool>.Success(hasHistory));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool>.Failure($"Failed to check script history: {ex.Message}"));
        }
    }

    private List<ScriptResponse> LoadScriptsFromFile()
    {
        if (!File.Exists(_historyFilePath))
            return new List<ScriptResponse>();

        try
        {
            var json = File.ReadAllText(_historyFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<ScriptResponse>();

            return JsonSerializer.Deserialize<List<ScriptResponse>>(json, _jsonOptions) ?? new List<ScriptResponse>();
        }
        catch
        {
            // If file is corrupted, start fresh
            return new List<ScriptResponse>();
        }
    }

    private void SaveScriptsToFile(List<ScriptResponse> scripts)
    {
        // Keep only the last 100 scripts to prevent file from growing too large
        if (scripts.Count > 100)
        {
            scripts = scripts.OrderByDescending(s => s.CreatedAt).Take(100).ToList();
        }

        var json = JsonSerializer.Serialize(scripts, _jsonOptions);
        File.WriteAllText(_historyFilePath, json);
    }
}
