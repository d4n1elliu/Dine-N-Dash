using DineNDash.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace DineNDash.Api.Middleware;

/// <summary>
/// Catches every unhandled error and turns it into a consistent error response.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var isAppError = exception is AppException;
        var statusCode = exception is AppException appException
            ? appException.StatusCode
            : StatusCodes.Status500InternalServerError;

        if (isAppError)
            logger.LogWarning("Request to {Path} failed: {Message}", httpContext.Request.Path, exception.Message);
        else
            logger.LogError(exception, "Unexpected error on {Path}", httpContext.Request.Path);

        var detail = isAppError || environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Detail = detail,
                Instance = httpContext.Request.Path
            }
        });
    }
}