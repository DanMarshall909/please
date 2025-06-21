using Please.Domain.Commands;
using Please.Domain.Common;

namespace Please.Domain.Interfaces;

/// <summary>
/// Provides contextual information for command processing and stores past patterns
/// </summary>
public interface IContextService
{
    Task<Result<CommandContext>> GetContextAsync(CommandIntent intent, CancellationToken cancellationToken = default);
    Task<Result> StorePatternAsync(CommandExecution execution, CancellationToken cancellationToken = default);
}
