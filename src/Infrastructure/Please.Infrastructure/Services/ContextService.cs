using Please.Domain.Commands;
using Please.Domain.Common;
using Please.Domain.Interfaces;

namespace Please.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of context service
/// </summary>
public class ContextService : IContextService
{
    public Task<Result<CommandContext>> GetContextAsync(CommandIntent intent,
        CancellationToken cancellationToken = default)
    {
        // Simple implementation - in real scenario this would gather system context
        var context = new CommandContext(Environment.CurrentDirectory);
        return Task.FromResult(Result<CommandContext>.Success(context));
    }

    public Task<VoidResult>
        StorePatternAsync(CommandExecution execution, CancellationToken cancellationToken = default) =>
        // Simple implementation - in real scenario this would store to database
        // For now, just return success
        VoidResult.SuccessfulTask;
}
