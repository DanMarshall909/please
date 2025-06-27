using System.Text.Json.Serialization;

namespace Please.Infrastructure.Serialization;

/// <summary>
/// Centralized JSON serialization context for all API providers.
/// Provides Native AOT and trimming-compatible serialization.
/// </summary>
[JsonSerializable(typeof(OpenAiRequest))]
[JsonSerializable(typeof(OpenAiResponse))]
[JsonSerializable(typeof(OpenAiChoice))]
[JsonSerializable(typeof(OpenAiMessage))]
[JsonSerializable(typeof(AnthropicRequest))]
[JsonSerializable(typeof(AnthropicResponse))]
[JsonSerializable(typeof(AnthropicContent))]
[JsonSerializable(typeof(AnthropicMessage))]
[JsonSerializable(typeof(OllamaRequest))]
[JsonSerializable(typeof(OllamaResponse))]
[JsonSerializable(typeof(OllamaRequestOptions))]
[JsonSerializable(typeof(GeminiRequest))]
[JsonSerializable(typeof(GeminiResponse))]
[JsonSerializable(typeof(GeminiCandidate))]
[JsonSerializable(typeof(GeminiContent))]
[JsonSerializable(typeof(GeminiPart))]
[JsonSerializable(typeof(GeminiContentItem))]
[JsonSerializable(typeof(GeminiGenerationConfig))]
[JsonSerializable(typeof(OpenRouterRequest))]
[JsonSerializable(typeof(OpenRouterResponse))]
[JsonSerializable(typeof(OpenRouterChoice))]
[JsonSerializable(typeof(OpenRouterMessage))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false
)]
public partial class ApiSerializationContext : JsonSerializerContext
{
}

// OpenAI Models (already defined in OpenAiProvider.cs but need to be public)
public class OpenAiRequest
{
    public string Model { get; set; } = string.Empty;
    public OpenAiMessage[] Messages { get; set; } = Array.Empty<OpenAiMessage>();
    public double Temperature { get; set; }
    [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
}

public class OpenAiResponse
{
    public OpenAiChoice[]? Choices { get; set; }
}

public class OpenAiChoice
{
    public OpenAiMessage? Message { get; set; }
}

public class OpenAiMessage
{
    public string Role { get; set; } = string.Empty;
    public string? Content { get; set; }
}

// Anthropic Models
public class AnthropicRequest
{
    public string Model { get; set; } = string.Empty;
    [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
    public string System { get; set; } = string.Empty;
    public AnthropicMessage[] Messages { get; set; } = Array.Empty<AnthropicMessage>();
}

public class AnthropicResponse
{
    public AnthropicContent[]? Content { get; set; }
}

public class AnthropicContent
{
    public string? Text { get; set; }
    public string? Type { get; set; }
}

public class AnthropicMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

// Ollama Models
public class OllamaRequest
{
    public string Model { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public bool Stream { get; set; }
    public OllamaRequestOptions? Options { get; set; }
}

public class OllamaResponse
{
    public string? Response { get; set; }
    public bool? Done { get; set; }
}

public class OllamaRequestOptions
{
    public double Temperature { get; set; }
    [JsonPropertyName("num_predict")] public int NumPredict { get; set; }
}

// Gemini Models
public class GeminiRequest
{
    public GeminiContentItem[] Contents { get; set; } = Array.Empty<GeminiContentItem>();
    [JsonPropertyName("generationConfig")] public GeminiGenerationConfig? GenerationConfig { get; set; }
}

public class GeminiResponse
{
    public GeminiCandidate[]? Candidates { get; set; }
}

public class GeminiCandidate
{
    public GeminiContent? Content { get; set; }
}

public class GeminiContent
{
    public GeminiPart[]? Parts { get; set; }
}

public class GeminiPart
{
    public string? Text { get; set; }
}

public class GeminiContentItem
{
    public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
}

public class GeminiGenerationConfig
{
    public double Temperature { get; set; }
    [JsonPropertyName("maxOutputTokens")] public int MaxOutputTokens { get; set; }
}

// OpenRouter Models
public class OpenRouterRequest
{
    public string Model { get; set; } = string.Empty;
    public OpenRouterMessage[] Messages { get; set; } = Array.Empty<OpenRouterMessage>();
    public double Temperature { get; set; }
    [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
}

public class OpenRouterResponse
{
    public OpenRouterChoice[]? Choices { get; set; }
}

public class OpenRouterChoice
{
    public OpenRouterMessage? Message { get; set; }
}

public class OpenRouterMessage
{
    public string Role { get; set; } = string.Empty;
    public string? Content { get; set; }
}
