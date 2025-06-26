using Microsoft.Extensions.Configuration;
using Please.Infrastructure.Providers;

namespace Please.Infrastructure.UnitTests.Configuration;

public class DependencyInjectionTests
{
    [Fact]
    public void Environment_variable_overrides_configuration_for_ollama_model()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OLLAMA_DEFAULT_MODEL", "custom-model");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DefaultSettings:OLLAMA_DEFAULT_MODEL"] = "config-model"
            })
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        try
        {
            // Act
            serviceCollection.AddInfrastructure();
            
            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var providerConfig = serviceProvider.GetRequiredService<ProviderConfiguration>();
            providerConfig.Ollama.DefaultModel.ShouldBe("custom-model");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("OLLAMA_DEFAULT_MODEL", null);
        }
    }

    [Fact]
    public void Configuration_used_when_no_environment_variable_for_ollama_model()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OLLAMA_DEFAULT_MODEL", null);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DefaultSettings:OLLAMA_DEFAULT_MODEL"] = "config-model"
            })
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        // Act
        serviceCollection.AddInfrastructure();
        
        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var providerConfig = serviceProvider.GetRequiredService<ProviderConfiguration>();
        providerConfig.Ollama.DefaultModel.ShouldBe("config-model");
    }

    [Fact]
    public void Fallback_to_default_when_no_environment_or_configuration_for_ollama_model()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OLLAMA_DEFAULT_MODEL", null);
        var configuration = new ConfigurationBuilder().Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        // Act
        serviceCollection.AddInfrastructure();
        
        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var providerConfig = serviceProvider.GetRequiredService<ProviderConfiguration>();
        providerConfig.Ollama.DefaultModel.ShouldBe("llama3:latest");
    }

    [Fact]
    public void Environment_variable_overrides_configuration_for_ollama_base_url()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OLLAMA_BASE_URL", "http://custom:8080");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DefaultSettings:OLLAMA_BASE_URL"] = "http://config:9090"
            })
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        try
        {
            // Act
            serviceCollection.AddInfrastructure();
            
            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var providerConfig = serviceProvider.GetRequiredService<ProviderConfiguration>();
            providerConfig.Ollama.BaseUrl.ShouldBe("http://custom:8080");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("OLLAMA_BASE_URL", null);
        }
    }

    [Fact]
    public void All_providers_are_configured_with_default_settings()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DefaultSettings:OPENAI_DEFAULT_MODEL"] = "gpt-4o-mini"
            })
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        // Act
        serviceCollection.AddInfrastructure();
        
        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var providerConfig = serviceProvider.GetRequiredService<ProviderConfiguration>();
        
        providerConfig.OpenAi.ShouldNotBeNull();
        providerConfig.Anthropic.ShouldNotBeNull();
        providerConfig.Gemini.ShouldNotBeNull();
        providerConfig.OpenRouter.ShouldNotBeNull();
        providerConfig.Ollama.ShouldNotBeNull();
        
        providerConfig.Ollama.BaseUrl.ShouldBe("http://localhost:11434");
        providerConfig.Ollama.DefaultModel.ShouldBe("llama3:latest");
    }
}