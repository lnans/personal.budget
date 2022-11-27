using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Models;

namespace Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JsonSerializerOptions _serializerOptions;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task InvokeAsync(HttpContext httpContext, ILogger<ExceptionMiddleware> logger)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception, logger);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ExceptionMiddleware> logger)
    {
        var error = new ErrorResponse
        {
            Type = exception.GetType().Name
        };

        switch (exception)
        {
            case ValidationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                error.Errors = ex.Errors.ToArray();
                break;
            case BadRequestException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                error.Errors = new[] { ex.Message };
                break;
            case NotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                error.Errors = new[] { ex.Message };
                break;
            case ConflictException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                error.Errors = new[] { ex.Message };
                break;
            case AuthenticationException _:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                error.Errors = new[] { "Authentication failed" };
                break;
            default:
                logger.LogError(exception, "Error during the request");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                error.Errors = new[] { "Unhandled error occurs" };
                break;
        }

        var result = JsonSerializer.Serialize(error, _serializerOptions);

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(result);
    }
}