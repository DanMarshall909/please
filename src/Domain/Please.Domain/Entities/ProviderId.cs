using Please.Domain.Common;

namespace Please.Domain.Entities;

public sealed record ProviderId(string Value) : StronglyTypedId<string>(Value)
{
    public static ProviderId OpenAI => new("openai");
    public static ProviderId Anthropic => new("anthropic");
    public static ProviderId Ollama => new("ollama");
}
