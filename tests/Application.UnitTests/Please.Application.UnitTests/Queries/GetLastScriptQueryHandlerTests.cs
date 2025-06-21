using TUnit;
using Please.TestUtilities;
using Please.Application.Queries.GetLastScript;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Queries;

[TestFixture]
public class GetLastScriptQueryHandlerTests
{
    private FakeScriptRepository _repository = null!;
    private GetLastScriptQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new FakeScriptRepository();
        _handler = new GetLastScriptQueryHandler(_repository);
    }

    [Test]
    public async Task Handle_Returns_last_script_from_repository()
    {
        var expected = ScriptResponse.Create("echo hi", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        await _repository.SaveScriptAsync(expected);

        var result = await _handler.Handle(GetLastScriptQuery.Create(), CancellationToken.None);

        Assert.Equal(expected, result);
    }
}

