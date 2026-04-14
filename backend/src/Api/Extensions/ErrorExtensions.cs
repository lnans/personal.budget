using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ErrorExtensions
{
    public static IResult ToOkResultOrProblem<TResponse>(this ErrorOr<TResponse> result, HttpContext httpContext) =>
        result.MatchFirst(Results.Ok, error => Results.Problem(error.ToProblem(httpContext)));

    public static ProblemDetails ToProblem(this Error error, HttpContext httpContext, Exception? exception = null) =>
        new()
        {
            Title = GetTitle(error),
            Detail = GetDetails(error),
            Type = GetTypeUri(error),
            Status = GetStatusCode(error),
            Extensions = GetExtensions(error, httpContext, exception),
        };

    private static string GetTitle(Error error) =>
        error.Type switch
        {
            ErrorType.Validation or ErrorType.Failure => "Validation Error",
            ErrorType.Conflict => "Conflict",
            ErrorType.NotFound => "Not Found",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Unexpected => "Internal Server Error",
            _ => throw new ArgumentOutOfRangeException(nameof(error), $"Unhandled error type: {error.Type}"),
        };

    private static string GetDetails(Error error) =>
        error.Type switch
        {
            ErrorType.Validation or ErrorType.Failure => "One or more validation errors occurred.",
            ErrorType.Conflict => "The request could not be completed due to a conflict.",
            ErrorType.NotFound => "The requested resource was not found.",
            ErrorType.Forbidden => "You do not have permission to access this resource.",
            ErrorType.Unauthorized => "Authentication is required to access this resource.",
            ErrorType.Unexpected => "An unexpected error occurred while processing the request.",
            _ => throw new ArgumentOutOfRangeException(nameof(error), $"Unhandled error type: {error.Type}"),
        };

    private static string GetTypeUri(Error error) =>
        error.Type switch
        {
            ErrorType.Validation or ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            ErrorType.Unexpected => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => throw new ArgumentOutOfRangeException(nameof(error), $"Unhandled error type: {error.Type}"),
        };

    private static int GetStatusCode(Error error) =>
        error.Type switch
        {
            ErrorType.Validation or ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => throw new ArgumentOutOfRangeException(nameof(error), $"Unhandled error type: {error.Type}"),
        };

    private static Dictionary<string, object?> GetExtensions(
        Error error,
        HttpContext httpContext,
        Exception? exception = null
    )
    {
        var extensions = new Dictionary<string, object?>
        {
            ["traceId"] = httpContext.TraceIdentifier,
            ["errors"] =
                error.Metadata?.Count > 0
                    ? error.Metadata
                    : new Dictionary<string, object> { [error.Code] = error.Description },
            ["exception"] = exception?.GetType().Name,
            ["stackTrace"] = FormatStackTrace(exception?.StackTrace),
            ["innerException"] = exception?.InnerException?.GetType().Name,
            ["innerStackTrace"] = FormatStackTrace(exception?.InnerException?.StackTrace),
        };

        var keysToRemove = extensions.Where(kv => kv.Value == null).Select(kv => kv.Key).ToList();
        foreach (var key in keysToRemove)
        {
            extensions.Remove(key);
        }

        return extensions;
    }

    private static string[]? FormatStackTrace(string? stackTrace) =>
        string.IsNullOrWhiteSpace(stackTrace)
        || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development"
            ? null
            : stackTrace.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();
}
