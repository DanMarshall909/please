using Xunit;
using Please.TestUtilities;
using Please.Application.Queries.GetLastScript;
using Please.Domain.Entities;
using Please.Domain.Enums;

namespace Please.Application.UnitTests.Queries;

public class GetLastScriptQueryHandlerTests
{
    private readonly FakeScriptRepository _repository;
    private readonly GetLastScriptQueryHandler _handler;

    public GetLastScriptQueryHandlerTests()
    {
        _repository = new FakeScriptRepository();
        _handler = new GetLastScriptQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_Returns_last_script_from_repository()
    {
        var expected = ScriptResponse.Create("echo hi", "task", ProviderType.OpenAI, "gpt-4", ScriptType.Bash);
        await _repository.SaveScriptAsync(expected);

        var result = await _handler.Handle(GetLastScriptQuery.Create(), CancellationToken.None);

        Assert.Equal(expected, result);
    }
}

