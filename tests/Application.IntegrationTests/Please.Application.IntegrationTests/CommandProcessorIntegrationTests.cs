using NUnit.Framework;
using Please.Application.Services;
using Please.TestUtilities;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Common;

namespace Please.Application.IntegrationTests;

[TestFixture]
public class CommandProcessorIntegrationTests
{
    [Test]
    public async Task process_async_flows_through_context_and_generator()
    {
        var context = new FakeContextService();
        var generator = new FakeScriptGenerator
        {
            NextResult = Result<ScriptResponse>.Success(
                ScriptResponse.Create("ls", "list", ProviderType.OpenAI, "gpt-4", ScriptType.Bash))
        };
        var processor = new CommandProcessor(context, generator);

        var result = await processor.ProcessAsync("list");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(generator.LastRequest?.TaskDescription, Is.EqualTo("list"));
    }
}
