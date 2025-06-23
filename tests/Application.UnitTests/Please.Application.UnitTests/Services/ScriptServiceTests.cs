using Xunit;
using Please.TestUtilities;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Please.Application.UnitTests.Services;

public class ScriptServiceTests
{
    private readonly FakeScriptGenerator _generator;
    private readonly FakeScriptRepository _repository;
    private readonly IScriptService _service;

    public ScriptServiceTests()
    {
        var provider = TestSystem.Create();
        _generator = provider.GetRequiredService<FakeScriptGenerator>();
        _repository = provider.GetRequiredService<FakeScriptRepository>();
        _service = provider.GetRequiredService<IScriptService>();
    }

    [Fact]
    public async Task generate_script_returns_failure_when_generation_fails()
    {
        var request = ScriptRequest.Create("test");
        _generator.NextResult = Result<ScriptResponse>.Failure("nope");

        var result = await _service.GenerateScriptAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal("nope", result.Error);
    }

    [Fact]
    public async Task generate_script_saves_and_returns_response_when_successful()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("echo hi", "task", ProviderType.OpenAi, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(response);

        var result = await _service.GenerateScriptAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(response, result.Value);
    }

    [Fact]
    public async Task generate_script_returns_failure_when_save_fails()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("script", "task", ProviderType.OpenAi, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(response);
        _repository.NextSaveResult = Result.Failure("db error");

        var result = await _service.GenerateScriptAsync(request);

        Assert.True(result.IsFailure);
        Assert.Contains("db error", result.Error);
    }
}
