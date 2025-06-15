using MediatR;
using Please.Domain.Entities;
using Please.Domain.Interfaces;

namespace Please.Application.Queries.GetLastScript;

/// <summary>
/// Handles queries to retrieve the last generated script
/// </summary>
public class GetLastScriptQueryHandler : IRequestHandler<GetLastScriptQuery, ScriptResponse?>
{
    private readonly IScriptRepository _scriptRepository;

    public GetLastScriptQueryHandler(IScriptRepository scriptRepository)
    {
        _scriptRepository = scriptRepository;
    }

    public async Task<ScriptResponse?> Handle(GetLastScriptQuery request, CancellationToken cancellationToken)
    {
        return await _scriptRepository.GetLastScriptAsync(cancellationToken);
    }
}
