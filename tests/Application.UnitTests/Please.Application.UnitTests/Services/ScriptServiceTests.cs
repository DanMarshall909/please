using NUnit.Framework;
using Please.TestUtilities;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Services;

[TestFixture]
public class ScriptServiceTests
{
    private FakeScriptGenerator _generator = null!;
    private FakeScriptRepository _repository = null!;
    private ScriptService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _generator = new FakeScriptGenerator();
        _repository = new FakeScriptRepository();
        _service = new ScriptService(_generator, _repository);
    }

    [Test]
    public async Task generate_script_returns_failure_when_generation_fails()
    {
        var request = ScriptRequest.Create("test");
        _generator.NextResult = Result<ScriptResponse>.Failure("nope");

        var result = await _service.GenerateScriptAsync(request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("nope"));
    }

    [Test]
    public async Task generate_script_saves_and_returns_response_when_successful()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("echo hi", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(response);

        var result = await _service.GenerateScriptAsync(request);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task generate_script_returns_failure_when_save_fails()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("script", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.NextResult = Result<ScriptResponse>.Success(response);
        _repository.NextSaveResult = Result.Failure("db error");

        var result = await _service.GenerateScriptAsync(request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("db error"));
    }
}
