using NUnit.Framework;
using Moq;
using Please.Application.Services;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Services;

[TestFixture]
public class ScriptServiceTests
{
    private Mock<IScriptGenerator> _generator = null!;
    private Mock<IScriptRepository> _repository = null!;
    private ScriptService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _generator = new Mock<IScriptGenerator>();
        _repository = new Mock<IScriptRepository>();
        _service = new ScriptService(_generator.Object, _repository.Object);
    }

    [Test]
    public async Task generate_script_returns_failure_when_generation_fails()
    {
        var request = ScriptRequest.Create("test");
        _generator.Setup(g => g.GenerateScriptAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScriptResponse>.Failure("nope"));

        var result = await _service.GenerateScriptAsync(request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("nope"));
    }

    [Test]
    public async Task generate_script_saves_and_returns_response_when_successful()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("echo hi", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.Setup(g => g.GenerateScriptAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScriptResponse>.Success(response));
        _repository.Setup(r => r.SaveScriptAsync(response, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _service.GenerateScriptAsync(request);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task generate_script_returns_failure_when_save_fails()
    {
        var request = ScriptRequest.Create("task");
        var response = ScriptResponse.Create("script", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _generator.Setup(g => g.GenerateScriptAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScriptResponse>.Success(response));
        _repository.Setup(r => r.SaveScriptAsync(response, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("db error"));

        var result = await _service.GenerateScriptAsync(request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("db error"));
    }
}
