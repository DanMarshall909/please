namespace Please.Infrastructure.Providers;

/// <summary>
/// Configuration for AI providers
/// </summary>
public class ProviderConfiguration
{
    /// <summary>
    /// OpenAI configuration
    /// </summary>
    public OpenAiConfiguration OpenAi { get; set; } = new();

    /// <summary>
    /// Anthropic configuration
    /// </summary>
    public AnthropicConfiguration Anthropic { get; set; } = new();

    /// <summary>
    /// Ollama configuration
    /// </summary>
    public OllamaConfiguration Ollama { get; set; } = new();

    /// <summary>
    /// OpenRouter configuration
    /// </summary>
    public OpenRouterConfiguration OpenRouter { get; set; } = new();

    /// <summary>
    /// Gemini configuration
    /// </summary>
    public GeminiConfiguration Gemini { get; set; } = new();
}

/// <summary>
/// OpenAI provider configuration
/// </summary>
public class OpenAiConfiguration
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string DefaultModel { get; set; } = "gpt-3.5-turbo";
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Anthropic provider configuration
/// </summary>
public class AnthropicConfiguration
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";
    public string DefaultModel { get; set; } = "claude-3-haiku-20240307";
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Ollama provider configuration
/// </summary>
public class OllamaConfiguration
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string DefaultModel { get; set; } = "llama3:latest";
    public int TimeoutSeconds { get; set; } = 60;
}

/// <summary>
/// OpenRouter provider configuration
/// </summary>
public class OpenRouterConfiguration
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string DefaultModel { get; set; } = "microsoft/wizardlm-2-8x22b";
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Gemini provider configuration
/// </summary>
public class GeminiConfiguration
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string DefaultModel { get; set; } = "gemini-pro";
    public int TimeoutSeconds { get; set; } = 30;
}
