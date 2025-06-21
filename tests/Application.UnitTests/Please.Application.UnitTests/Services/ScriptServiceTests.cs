using TUnit;
using Please.TestUtilities;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Please.Application;

namespace Please.Application.UnitTests.Services;

[TestFixture]
public class ScriptServiceTests
{
    private FakeScriptGenerator _generator = null!;
    private FakeScriptRepository _repository = null!;
    private IServiceProvider _provider = null!;
    private IScriptService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = TestSystem.Create();
        _generator = _provider.GetRequiredService<FakeScriptGenerator>();
        _repository = _provider.GetRequiredService<FakeScriptRepository>();
        _service = _provider.GetRequiredService<IScriptService>();
    }

    [Test]
    public async Task generate_script_returns_failure_when_generation_fails()
    {
        var request = ScriptRequest.Create("test");
        _generator.NextResult = Result<ScriptResponse>.Failure("nope");

        var result = await _service.GenerateScriptAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal("nope", result.Error);
    }

    [Test]
    public async Task generate_script_saves_and_returns_response_when_successful()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("echo hi", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(response);

        var result = await _service.GenerateScriptAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(response, result.Value);
    }

    [Test]
    public async Task generate_script_returns_failure_when_save_fails()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("script", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(response);
        _repository.NextSaveResult = Result.Failure("db error");

        var result = await _service.GenerateScriptAsync(request);

        Assert.True(result.IsFailure);
        Assert.Contains("db error", result.Error);
    }
}
