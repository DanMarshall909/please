using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.UnitTests;

public class ScriptServiceTests
{
    private sealed class FakeGenerator(bool shouldFail) : IScriptGenerator
    {
        public Task<Result<ScriptResponse>> GenerateScriptAsync(ScriptRequest request, CancellationToken cancellationToken = default)
            => shouldFail
                ? Task.FromResult(Result<ScriptResponse>.Failure("gen fail"))
                : Task.FromResult(Result<ScriptResponse>.Success(
                    ScriptResponse.Create("cmd", request.TaskDescription, ProviderType.OpenAI, "gpt-4").Value!));

        public Task<bool> IsProviderAvailableAsync(ScriptRequest request, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public string GetFallbackModel(ScriptRequest request) => "gpt-4";
    }

    private sealed class FakeRepository(bool shouldFail) : IScriptRepository
    {
        public Task<Result> SaveScriptAsync(ScriptResponse response, CancellationToken cancellationToken = default)
            => shouldFail ? Task.FromResult(Result.Failure("repo fail")) : Task.FromResult(Result.Success());

        public Task<Result<ScriptResponse>> GetLastScriptAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result<ScriptResponse>.Failure("no script"));

        public Task<Result<IEnumerable<ScriptResponse>>> GetScriptHistoryAsync(int? count = null, DateTime? since = null, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<IEnumerable<ScriptResponse>>.Success(Array.Empty<ScriptResponse>()));

        public Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default) => Task.FromResult(Result.Success());
        public Task<bool> HasHistoryAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    [Fact]
    public async Task Test_generate_script_async_returns_saved_script_on_success()
    {
        var generator = new FakeGenerator(false);
        var repository = new FakeRepository(false);
        var service = new ScriptService(generator, repository);

        var request = ScriptRequest.Create("task").Value!;
        var result = await service.GenerateScriptAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TaskDescription.Should().Be("task");
    }

    [Fact]
    public async Task Test_generate_script_async_returns_failure_when_generator_fails()
    {
        var generator = new FakeGenerator(true);
        var repository = new FakeRepository(false);
        var service = new ScriptService(generator, repository);

        var request = ScriptRequest.Create("task").Value!;
        var result = await service.GenerateScriptAsync(request);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Test_generate_script_async_returns_failure_when_repository_fails()
    {
        var generator = new FakeGenerator(false);
        var repository = new FakeRepository(true);
        var service = new ScriptService(generator, repository);

        var request = ScriptRequest.Create("task").Value!;
        var result = await service.GenerateScriptAsync(request);

        result.IsFailure.Should().BeTrue();
    }
}
