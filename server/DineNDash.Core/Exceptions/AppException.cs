namespace DineNDash.Core.Exceptions;

/// <summary>
/// Errors the API turns into a ProblemDetails response using the subclass status code; anything else becomes a 500
/// </summary>
public abstract class AppException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }
}