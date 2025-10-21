using Microsoft.AspNetCore.Mvc;

namespace Api.Errors;

public static class ProblemExceptionBuilder
{
    public static void AddExceptionDetails(ProblemDetails details, Exception exception)
    {
        details.Extensions["exception"] = exception.GetType().Name;
        AddStackTraceIfAvailable(details, "stackTrace", exception.StackTrace);

        if (exception.InnerException is not null)
        {
            AddInnerExceptionDetails(details, exception.InnerException);
        }
    }

    private static void AddInnerExceptionDetails(ProblemDetails details, Exception innerException)
    {
        details.Extensions["innerException"] = innerException.GetType().Name;
        AddStackTraceIfAvailable(details, "innerStackTrace", innerException.StackTrace);
    }

    private static void AddStackTraceIfAvailable(ProblemDetails details, string key, string? stackTrace)
    {
        var lines = FormatStackTrace(stackTrace);
        if (lines.Length > 0)
        {
            details.Extensions[key] = lines;
        }
    }

    private static string[] FormatStackTrace(string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
        {
            return [];
        }

        return stackTrace.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();
    }
}
