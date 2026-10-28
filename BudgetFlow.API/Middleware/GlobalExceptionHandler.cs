using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BudgetFlow.API.Middleware;

/// <summary>
/// Centralized exception handler that converts unhandled exceptions into a
/// consistent <see cref="ProblemDetails"/> response (RFC 7807).
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred while processing {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        var (statusCode, title) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ValidationException => ((int)HttpStatusCode.BadRequest, "Validation Failed"),
        ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid Argument"),
        KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource Not Found"),
        UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
        _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred"),
    };
}
