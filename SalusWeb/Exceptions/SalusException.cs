namespace SalusWeb.Exceptions;

/// <summary>
/// Base exception for Salus iT600 Gateway errors
/// </summary>
public class SalusException : Exception
{
    public SalusException(string message) : base(message) { }
    public SalusException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when authentication with the gateway fails
/// </summary>
public class SalusAuthenticationException : SalusException
{
    public SalusAuthenticationException(string message) : base(message) { }
    public SalusAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when connection to the gateway fails
/// </summary>
public class SalusConnectionException : SalusException
{
    public SalusConnectionException(string message) : base(message) { }
    public SalusConnectionException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a command to the gateway fails
/// </summary>
public class SalusCommandException : SalusException
{
    public SalusCommandException(string message) : base(message) { }
    public SalusCommandException(string message, Exception innerException) : base(message, innerException) { }
}
