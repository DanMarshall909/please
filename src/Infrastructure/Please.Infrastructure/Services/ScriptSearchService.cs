using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Please.Infrastructure.Services;

/// <summary>
/// Implementation of script search functionality with advanced filtering and indexing
/// </summary>
public sealed class ScriptSearchService : IScriptSearchService
{
    private readonly IScriptRepository _repository;
    private readonly ILogger<ScriptSearchService> _logger;
    private readonly Dictionary<string, IEnumerable<ScriptResponse>> _searchCache;
    private readonly object _cacheLock = new();
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public ScriptSearchService(IScriptRepository repository, ILogger<ScriptSearchService> logger)
    {
        _repository = repository;
        _logger = logger;
        _searchCache = new Dictionary<string, IEnumerable<ScriptResponse>>();
    }

    public async Task<Result<IEnumerable<ScriptResponse>>> SearchAsync(
        string query,
        SearchOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= SearchOptions.Default;
            
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetFilteredScriptsAsync(options, cancellationToken);
            }

            _logger.LogInformation("Searching scripts with query: '{Query}'", query);

            // Check cache first
            var cacheKey = GenerateCacheKey(query, options);
            if (TryGetFromCache(cacheKey, out var cachedResult))
            {
                _logger.LogDebug("Returning cached search results for query: '{Query}'", query);
                return Result<IEnumerable<ScriptResponse>>.Success(cachedResult);
            }

            // Get all scripts from repository
            var getAllResult = await _repository.GetAllScriptsAsync(cancellationToken);
            if (!getAllResult.IsSuccess)
            {
                _logger.LogError("Failed to retrieve scripts from repository: {Error}", getAllResult.Error);
                return Result<IEnumerable<ScriptResponse>>.Failure(getAllResult.Error);
            }

            var allScripts = getAllResult.Value!;

            // Apply search filtering
            var searchResults = ApplySearch(allScripts, query, options);

            // Cache the results
            CacheResults(cacheKey, searchResults);

            _logger.LogInformation("Found {Count} scripts matching query: '{Query}'", searchResults.Count(), query);
            return Result<IEnumerable<ScriptResponse>>.Success(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching scripts with query: '{Query}'", query);
            return Result<IEnumerable<ScriptResponse>>.Failure($"Search failed: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ScriptResponse>>> GetFilteredScriptsAsync(
        SearchOptions? options = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= SearchOptions.Default;
            
            _logger.LogDebug("Getting filtered scripts with options");

            // Get all scripts from repository
            var getAllResult = await _repository.GetAllScriptsAsync(cancellationToken);
            if (!getAllResult.IsSuccess)
            {
                _logger.LogError("Failed to retrieve scripts from repository: {Error}", getAllResult.Error);
                return Result<IEnumerable<ScriptResponse>>.Failure(getAllResult.Error);
            }

            var allScripts = getAllResult.Value!;

            // Apply filtering only (no text search)
            var filteredScripts = ApplyFilters(allScripts, options);

            _logger.LogInformation("Found {Count} scripts matching filters", filteredScripts.Count());
            return Result<IEnumerable<ScriptResponse>>.Success(filteredScripts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while filtering scripts");
            return Result<IEnumerable<ScriptResponse>>.Failure($"Filtering failed: {ex.Message}");
        }
    }

    public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string partialQuery, int maxSuggestions = 5)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
            {
                return Enumerable.Empty<string>();
            }

            _logger.LogDebug("Getting search suggestions for: '{PartialQuery}'", partialQuery);

            // Get all scripts from repository
            var getAllResult = await _repository.GetAllScriptsAsync();
            if (!getAllResult.IsSuccess)
            {
                return Enumerable.Empty<string>();
            }

            var allScripts = getAllResult.Value!;

            // Extract unique words from task descriptions
            var suggestions = allScripts
                .SelectMany(script => ExtractWords(script.TaskDescription))
                .Where(word => word.StartsWith(partialQuery, StringComparison.OrdinalIgnoreCase))
                .GroupBy(word => word.ToLowerInvariant())
                .OrderByDescending(group => group.Count()) // Most frequent first
                .ThenBy(group => group.Key) // Then alphabetically
                .Take(maxSuggestions)
                .Select(group => group.First())
                .ToList();

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting search suggestions");
            return Enumerable.Empty<string>();
        }
    }

    private IEnumerable<ScriptResponse> ApplySearch(IEnumerable<ScriptResponse> scripts, string query, SearchOptions options)
    {
        // First apply filters
        var filtered = ApplyFilters(scripts, options);

        // Then apply text search
        var searchResults = ApplyTextSearch(filtered, query, options);

        return searchResults;
    }

    private IEnumerable<ScriptResponse> ApplyFilters(IEnumerable<ScriptResponse> scripts, SearchOptions options)
    {
        var filtered = scripts.AsEnumerable();

        // Date filtering
        if (options.FromDate.HasValue)
        {
            filtered = filtered.Where(s => s.CreatedAt >= options.FromDate.Value);
        }

        if (options.ToDate.HasValue)
        {
            filtered = filtered.Where(s => s.CreatedAt <= options.ToDate.Value);
        }

        // Provider filtering
        if (options.Provider.HasValue)
        {
            filtered = filtered.Where(s => s.Provider == options.Provider.Value);
        }

        // Risk level filtering
        if (options.MaxRiskLevel.HasValue)
        {
            filtered = filtered.Where(s => s.RiskLevel <= options.MaxRiskLevel.Value);
        }

        // Script type filtering
        if (options.ScriptType.HasValue)
        {
            filtered = filtered.Where(s => s.ScriptType == options.ScriptType.Value);
        }

        // Apply sorting
        filtered = ApplySorting(filtered, options.SortOrder);

        // Apply pagination
        if (options.Skip > 0)
        {
            filtered = filtered.Skip(options.Skip);
        }

        if (options.MaxResults > 0)
        {
            filtered = filtered.Take(options.MaxResults);
        }

        return filtered;
    }

    private IEnumerable<ScriptResponse> ApplyTextSearch(IEnumerable<ScriptResponse> scripts, string query, SearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return scripts;
        }

        var comparison = options.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        IEnumerable<ScriptResponse> searchResults;

        if (options.UseRegex)
        {
            // Regex search
            try
            {
                var regexOptions = options.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                var regex = new Regex(query, regexOptions);

                searchResults = scripts.Where(script =>
                    regex.IsMatch(script.TaskDescription) ||
                    (options.SearchInContent && regex.IsMatch(script.Script))
                ).ToList();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid regex pattern '{Query}': {Error}", query, ex.Message);
                // Fall back to simple text search
                searchResults = ApplySimpleTextSearch(scripts, query, options, comparison);
            }
        }
        else
        {
            // Simple text search
            searchResults = ApplySimpleTextSearch(scripts, query, options, comparison);
        }

        // Calculate relevance scores for sorting
        if (options.SortOrder == SearchSortOrder.Relevance)
        {
            searchResults = searchResults
                .Select(script => new { Script = script, Score = CalculateRelevanceScore(script, query, options) })
                .OrderByDescending(item => item.Score)
                .Select(item => item.Script);
        }

        return searchResults;
    }

    private IEnumerable<ScriptResponse> ApplySimpleTextSearch(IEnumerable<ScriptResponse> scripts, string query, SearchOptions options, StringComparison comparison)
    {
        var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return scripts.Where(script =>
        {
            var taskDescription = script.TaskDescription;
            var scriptContent = options.SearchInContent ? script.Script : string.Empty;

            // All search terms must be found (AND logic)
            return searchTerms.All(term =>
                taskDescription.Contains(term, comparison) ||
                (options.SearchInContent && scriptContent.Contains(term, comparison))
            );
        });
    }

    private IEnumerable<ScriptResponse> ApplySorting(IEnumerable<ScriptResponse> scripts, SearchSortOrder sortOrder)
    {
        return sortOrder switch
        {
            SearchSortOrder.CreatedDateAscending => scripts.OrderBy(s => s.CreatedAt),
            SearchSortOrder.CreatedDateDescending => scripts.OrderByDescending(s => s.CreatedAt),
            SearchSortOrder.TaskDescriptionAscending => scripts.OrderBy(s => s.TaskDescription),
            SearchSortOrder.TaskDescriptionDescending => scripts.OrderByDescending(s => s.TaskDescription),
            SearchSortOrder.ProviderAscending => scripts.OrderBy(s => s.Provider),
            SearchSortOrder.ProviderDescending => scripts.OrderByDescending(s => s.Provider),
            SearchSortOrder.RiskLevelAscending => scripts.OrderBy(s => s.RiskLevel),
            SearchSortOrder.RiskLevelDescending => scripts.OrderByDescending(s => s.RiskLevel),
            SearchSortOrder.Relevance => scripts, // Relevance sorting is handled in text search
            _ => scripts.OrderByDescending(s => s.CreatedAt)
        };
    }

    private double CalculateRelevanceScore(ScriptResponse script, string query, SearchOptions options)
    {
        double score = 0.0;
        var queryTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var comparison = options.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach (var term in queryTerms)
        {
            // Task description matches (higher weight)
            var taskMatches = CountOccurrences(script.TaskDescription, term, comparison);
            score += taskMatches * 3.0;

            // Script content matches (lower weight)
            if (options.SearchInContent)
            {
                var contentMatches = CountOccurrences(script.Script, term, comparison);
                score += contentMatches * 1.0;
            }

            // Bonus for exact phrase match in task description
            if (script.TaskDescription.Contains(query, comparison))
            {
                score += 5.0;
            }
        }

        // Recency bonus (newer scripts get slight boost)
        var daysSinceCreated = (DateTime.Now - script.CreatedAt).TotalDays;
        var recencyBonus = Math.Max(0, 1.0 - (daysSinceCreated / 30.0)); // Bonus decreases over 30 days
        score += recencyBonus;

        return score;
    }

    private int CountOccurrences(string text, string searchTerm, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
            return 0;

        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(searchTerm, index, comparison)) != -1)
        {
            count++;
            index += searchTerm.Length;
        }

        return count;
    }

    private IEnumerable<string> ExtractWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Enumerable.Empty<string>();

        return Regex.Matches(text, @"\b\w{3,}\b") // Words with 3+ characters
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(word => !IsCommonWord(word))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private bool IsCommonWord(string word)
    {
        var commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "its", "may", "new", "now", "old", "see", "two", "who", "boy", "did", "did", "she", "use", "her", "way", "who", "oil", "sit", "set", "run", "eat", "far", "sea", "eye", "run", "ask", "let", "try", "say", "yes", "yet", "tell", "each", "even", "back", "good", "life", "very", "well", "down", "just", "much", "only", "over", "such", "take", "than", "them", "time", "work"
        };

        return commonWords.Contains(word);
    }

    private string GenerateCacheKey(string query, SearchOptions options)
    {
        var key = $"{query}|{options.FromDate}|{options.ToDate}|{options.Provider}|{options.MaxRiskLevel}|{options.ScriptType}|{options.MaxResults}|{options.Skip}|{options.SortOrder}|{options.SearchInContent}|{options.CaseSensitive}|{options.UseRegex}";
        return key;
    }

    private bool TryGetFromCache(string cacheKey, out IEnumerable<ScriptResponse> result)
    {
        result = Enumerable.Empty<ScriptResponse>();

        lock (_cacheLock)
        {
            if (DateTime.Now - _lastCacheUpdate > _cacheExpiry)
            {
                _searchCache.Clear();
                return false;
            }

            return _searchCache.TryGetValue(cacheKey, out result!);
        }
    }

    private void CacheResults(string cacheKey, IEnumerable<ScriptResponse> results)
    {
        lock (_cacheLock)
        {
            _searchCache[cacheKey] = results.ToList(); // Materialize the results
            _lastCacheUpdate = DateTime.Now;

            // Limit cache size
            if (_searchCache.Count > 100)
            {
                var oldestKey = _searchCache.Keys.First();
                _searchCache.Remove(oldestKey);
            }
        }
    }
}