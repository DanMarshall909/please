using Please.Domain.Commands;
using Please.Domain.Common;
using Please.Domain.Interfaces;

namespace Please.TestUtilities;

public sealed class FakeContextService : IContextService
{
    public Result<CommandContext> ContextResult { get; set; } =
        Result<CommandContext>.Success(new CommandContext("/"));

    public List<CommandExecution> StoredExecutions { get; } = new();

    public Task<Result<CommandContext>> GetContextAsync(CommandIntent intent, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ContextResult);
    }

    public Task<Result> StorePatternAsync(CommandExecution execution, CancellationToken cancellationToken = default)
    {
        StoredExecutions.Add(execution);
        return Task.FromResult(Result.Success());
    }
}
