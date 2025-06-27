using System.Text.Json;
using Please.Domain.Entities;
using Please.Infrastructure.Serialization;
using static Please.Infrastructure.Serialization.ApiSerializationContext;

namespace Please.Infrastructure.UnitTests.Providers;

/// <summary>
/// Tests to verify Native AOT compatible JSON serialization for all API providers
/// </summary>
public class JsonSerializationTests
{
    [Fact]
    public void Test_openai_request_serialization_works_with_source_generation()
    {
        // Given: An OpenAI request object
        var request = new ScriptRequest
        {
            TaskDescription = "list all files",
            WorkingDirectory = "/test/directory",
            ScriptType = Domain.Enums.ScriptType.PowerShell,
            Model = "gpt-4o"
        };

        // When: We try to serialize using the ApiSerializationContext
        var exception = Record.Exception(() =>
        {
            var requestBody = new OpenAiRequest
            {
                Model = request.Model ?? "gpt-4o",
                Messages = new[]
                {
                    new OpenAiMessage { Role = "system", Content = "Test system prompt" },
                    new OpenAiMessage { Role = "user", Content = request.TaskDescription }
                },
                Temperature = 0.1,
                MaxTokens = 1000
            };

            // This should work with source generation
            string json = JsonSerializer.Serialize(requestBody, Default.OpenAiRequest);
        });

        // Then: Should not throw any serialization exceptions
        Assert.Null(exception);
    }

    [Fact]
    public void Test_openai_response_deserialization_works_with_source_generation()
    {
        // Given: A sample OpenAI API response JSON
        var responseJson = """
                           {
                               "choices": [
                                   {
                                       "message": {
                                           "role": "assistant",
                                           "content": "Get-ChildItem -Path . -Recurse"
                                       }
                                   }
                               ]
                           }
                           """;

        // When: We try to deserialize using the ApiSerializationContext
        var exception = Record.Exception(() =>
        {
            var response = JsonSerializer.Deserialize(responseJson, Default.OpenAiResponse);

            // Verify the content was parsed correctly
            Assert.NotNull(response);
            Assert.NotNull(response.Choices);
            Assert.Single(response.Choices);
            Assert.Equal("Get-ChildItem -Path . -Recurse", response.Choices[0].Message?.Content);
        });

        // Then: Should not throw any deserialization exceptions
        Assert.Null(exception);
    }

    [Fact]
    public void Test_anthropic_request_serialization_works_with_source_generation()
    {
        // Given: An Anthropic request object
        var request = new AnthropicRequest
        {
            Model = "claude-3-5-sonnet-20241022",
            MaxTokens = 1000,
            System = "You are a helpful assistant",
            Messages = new[]
            {
                new AnthropicMessage { Role = "user", Content = "Help me write a script" }
            }
        };

        // When: We try to serialize using the ApiSerializationContext
        var exception = Record.Exception(() =>
        {
            string json = JsonSerializer.Serialize(request, Default.AnthropicRequest);

            // Verify the JSON contains expected properties
            Assert.NotNull(json);
            Assert.Contains("claude-3-5-sonnet-20241022", json);
            Assert.Contains("max_tokens", json); // Should be snake_case
        });

        // Then: Should not throw any serialization exceptions
        Assert.Null(exception);
    }
}
