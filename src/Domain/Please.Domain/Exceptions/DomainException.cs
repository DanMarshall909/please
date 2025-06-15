namespace Please.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-specific errors
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when an unsupported AI provider is requested
/// </summary>
public class UnsupportedProviderException : DomainException
{
    public UnsupportedProviderException(string provider) 
        : base($"Unsupported provider: {provider}") { }
}

/// <summary>
/// Thrown when an unsupported model is requested for a provider
/// </summary>
public class UnsupportedModelException : DomainException
{
    public UnsupportedModelException(string provider, string model) 
        : base($"Model '{model}' is not supported by provider '{provider}'") { }
}

/// <summary>
/// Thrown when script generation fails
/// </summary>
public class ScriptGenerationException : DomainException
{
    public ScriptGenerationException(string message) : base(message) { }
    public ScriptGenerationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when script validation fails
/// </summary>
public class ScriptValidationException : DomainException
{
    public ScriptValidationException(string message) : base(message) { }
    public ScriptValidationException(string message, Exception innerException) : base(message, innerException) { }
}
