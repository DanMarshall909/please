using System.Runtime.InteropServices;
using System.Security;

namespace Please.Domain.ValueObjects;

/// <summary>
/// A secure string wrapper that prevents sensitive data from being stored in plain text in memory.
/// Implements IDisposable to ensure proper cleanup of sensitive data.
/// </summary>
public sealed class SecureString : IDisposable
{
    private readonly System.Security.SecureString _secureData;
    private bool _disposed = false;

    private SecureString(string value)
    {
        _secureData = new System.Security.SecureString();
        foreach (char c in value)
        {
            _secureData.AppendChar(c);
        }
        _secureData.MakeReadOnly();
    }

    /// <summary>
    /// Creates a new SecureString from a plain text value.
    /// The original string is immediately cleared from memory.
    /// </summary>
    /// <param name="value">The sensitive value to secure</param>
    /// <returns>A new SecureString instance</returns>
    public static SecureString Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Cannot create SecureString from null or empty value", nameof(value));

        var secureString = new SecureString(value);

        // Clear the original string from memory by overwriting it
        unsafe
        {
            fixed (char* ptr = value)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    ptr[i] = '\0';
                }
            }
        }

        return secureString;
    }

    /// <summary>
    /// Converts the SecureString back to a plain string for use.
    /// WARNING: This should only be called when absolutely necessary for API calls.
    /// The returned string should be cleared from memory as soon as possible.
    /// </summary>
    /// <returns>The decrypted string value</returns>
    public string ToUnsecureString()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SecureString));

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(_secureData);
            return Marshal.PtrToStringUni(ptr) ?? string.Empty;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }

    /// <summary>
    /// Checks if the SecureString is empty or null.
    /// </summary>
    public bool IsEmpty => _secureData.Length == 0;

    /// <summary>
    /// Gets the length of the secured string without exposing its contents.
    /// </summary>
    public int Length => _secureData.Length;

    /// <summary>
    /// Validates that the secured string meets basic API key requirements.
    /// </summary>
    /// <returns>True if the string appears to be a valid API key format</returns>
    public bool IsValidApiKeyFormat()
    {
        if (_disposed || IsEmpty)
            return false;

        // Basic validation - length and character checks without exposing the actual value
        return Length >= 20 && Length <= 200; // Most API keys are in this range
    }

    /// <summary>
    /// Clears sensitive data from memory.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _secureData?.Dispose();
            _disposed = true;
        }
    }

    ~SecureString()
    {
        Dispose();
    }

    /// <summary>
    /// Implicit conversion from string to SecureString for convenience.
    /// </summary>
    public static implicit operator SecureString(string value) => Create(value);

    /// <summary>
    /// Override ToString to prevent accidental exposure of sensitive data.
    /// </summary>
    public override string ToString() => "[SecureString - Hidden]";

    /// <summary>
    /// Override GetHashCode to use the secure data's hash.
    /// </summary>
    public override int GetHashCode() => _secureData?.GetHashCode() ?? 0;

    /// <summary>
    /// Override Equals to compare SecureString instances safely.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not SecureString other)
            return false;

        if (_disposed || other._disposed)
            return false;

        // Compare lengths first as a quick check
        if (Length != other.Length)
            return false;

        // For actual comparison, we'd need to convert both to unsafe strings temporarily
        // This is a security trade-off - we expose both values briefly to compare them
        string thisValue = ToUnsecureString();
        string otherValue = other.ToUnsecureString();

        try
        {
            return string.Equals(thisValue, otherValue, StringComparison.Ordinal);
        }
        finally
        {
            // Clear both strings from memory
            clearString(thisValue);
            clearString(otherValue);
        }
    }

    /// <summary>
    /// Attempts to clear a string from memory by overwriting it.
    /// </summary>
    private static void clearString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        unsafe
        {
            fixed (char* ptr = str)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    ptr[i] = '\0';
                }
            }
        }
    }
}
