using MediatR;
using Please.Domain.Entities;

namespace Please.Application.Queries.GetLastScript;

/// <summary>
/// Query to retrieve the last generated script
/// </summary>
public record GetLastScriptQuery : IRequest<ScriptResponse?>
{
    /// <summary>
    /// Creates a new instance of GetLastScriptQuery
    /// </summary>
    public static GetLastScriptQuery Create() => new();
}
