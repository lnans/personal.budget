using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Api.Errors;

public static class Problems
{
    public const string ProblemContentType = "application/problem+json";

    public static ProblemDetails Failure(HttpContext httpContext, string? detail = null)
    {
        var details = new ProblemDetails
        {
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = detail ?? "The request could not be processed.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Extensions = { ["traceId"] = httpContext.TraceIdentifier },
        };

        return details;
    }

    public static ProblemDetails Validation(HttpContext httpContext, IDictionary<string, object>? errors = null)
    {
        var details = new ProblemDetails
        {
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Extensions = { ["traceId"] = httpContext.TraceIdentifier },
        };

        if (errors is not null && errors.Count > 0)
        {
            details.Extensions["errors"] = errors;
        }

        return details;
    }

    public static ProblemDetails Unauthorized(HttpContext httpContext)
    {
        var details = new ProblemDetails
        {
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "Authentication is required to access this resource.",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Extensions = { ["traceId"] = httpContext.TraceIdentifier },
        };

        return details;
    }

    public static ProblemDetails Forbidden(HttpContext httpContext)
    {
        var details = new ProblemDetails
        {
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = "You do not have permission to access this resource.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Extensions = { ["traceId"] = httpContext.TraceIdentifier },
        };

        return details;
    }

    public static ProblemDetails NotFound(HttpContext httpContext, string? detail = null)
    {
        var details = new ProblemDetails
        {
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = detail ?? "The requested resource was not found.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Extensions = { ["traceId"] = httpContext.TraceIdentifier },
        };

        return details;
    }

    public static ProblemDetails Conflict(HttpContext httpContext, string? detail = null)
    {
        var details = new ProblemDetails
        {
            Title = "Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = detail ?? "The request conflicts with the current state of the resource.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Extensions = { ["traceId"] = httpContext.TraceIdentifier },
        };

        return details;
    }

    public static ProblemDetails InternalServerError(Exception? exception, HttpContext httpContext)
    {
        var details = new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception?.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Extensions = { ["traceId"] = httpContext.TraceIdentifier },
        };

        if (exception is not null)
        {
            ProblemExceptionBuilder.AddExceptionDetails(details, exception);
        }

        return details;
    }
}
