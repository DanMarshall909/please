using NUnit.Framework;
using Moq;
using Please.Application.Queries.GetLastScript;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Queries;

[TestFixture]
public class GetLastScriptQueryHandlerTests
{
    private Mock<IScriptRepository> _repository = null!;
    private GetLastScriptQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IScriptRepository>();
        _handler = new GetLastScriptQueryHandler(_repository.Object);
    }

    [Test]
    public async Task Handle_Returns_last_script_from_repository()
    {
        var expected = ScriptResponse.Create("echo hi", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        _repository.Setup(r => r.GetLastScriptAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScriptResponse?>.Success(expected));

        var result = await _handler.Handle(GetLastScriptQuery.Create(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
    }
}

