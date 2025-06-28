using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Domain.Interfaces;

/// <summary>
/// Service for searching and filtering scripts in the repository
/// </summary>
public interface IScriptSearchService
{
    /// <summary>
    /// Search scripts based on query and filtering options
    /// </summary>
    /// <param name="query">Search query to match against task descriptions and script content</param>
    /// <param name="options">Search filtering options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing matching scripts</returns>
    Task<Result<IEnumerable<ScriptResponse>>> SearchAsync(
        string query,
        SearchOptions? options = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all available scripts with optional filtering
    /// </summary>
    /// <param name="options">Filtering options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing filtered scripts</returns>
    Task<Result<IEnumerable<ScriptResponse>>> GetFilteredScriptsAsync(
        SearchOptions? options = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get search suggestions based on partial query
    /// </summary>
    /// <param name="partialQuery">Partial search query</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return</param>
    /// <returns>List of search suggestions</returns>
    Task<IEnumerable<string>> GetSearchSuggestionsAsync(string partialQuery, int maxSuggestions = 5);
}

/// <summary>
/// Options for filtering and searching scripts
/// </summary>
public class SearchOptions
{
    /// <summary>
    /// Filter scripts created from this date onwards
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter scripts created up to this date
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by AI provider that generated the script
    /// </summary>
    public ProviderType? Provider { get; set; }

    /// <summary>
    /// Filter by maximum risk level (inclusive)
    /// </summary>
    public RiskLevel? MaxRiskLevel { get; set; }

    /// <summary>
    /// Filter by script type
    /// </summary>
    public ScriptType? ScriptType { get; set; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int MaxResults { get; set; } = 50;

    /// <summary>
    /// Skip this many results (for pagination)
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Sort order for results
    /// </summary>
    public SearchSortOrder SortOrder { get; set; } = SearchSortOrder.CreatedDateDescending;

    /// <summary>
    /// Include script content in search (not just task descriptions)
    /// </summary>
    public bool SearchInContent { get; set; } = false;

    /// <summary>
    /// Case-sensitive search
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Use regex for advanced pattern matching
    /// </summary>
    public bool UseRegex { get; set; } = false;

    /// <summary>
    /// Default options for basic search
    /// </summary>
    public static SearchOptions Default => new();

    /// <summary>
    /// Options for recent scripts (last 7 days)
    /// </summary>
    public static SearchOptions Recent => new()
    {
        FromDate = DateTime.Now.AddDays(-7),
        SortOrder = SearchSortOrder.CreatedDateDescending
    };

    /// <summary>
    /// Options for safe scripts only (Low and Medium risk)
    /// </summary>
    public static SearchOptions SafeOnly => new()
    {
        MaxRiskLevel = RiskLevel.Medium,
        SortOrder = SearchSortOrder.CreatedDateDescending
    };
}

/// <summary>
/// Sort order options for search results
/// </summary>
public enum SearchSortOrder
{
    CreatedDateAscending,
    CreatedDateDescending,
    TaskDescriptionAscending,
    TaskDescriptionDescending,
    ProviderAscending,
    ProviderDescending,
    RiskLevelAscending,
    RiskLevelDescending,
    Relevance
}