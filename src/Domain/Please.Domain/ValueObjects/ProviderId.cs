namespace Please.Domain.Entities;

public readonly record struct ProviderId(string Value)
{
    public static readonly ProviderId OpenAi = new("openai");
    public static readonly ProviderId Anthropic = new("anthropic");
    public static readonly ProviderId Ollama = new("ollama");
    public static implicit operator string(ProviderId providerId) => providerId.Value;
    public override string ToString() => Value;
}
