namespace Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JsonSerializerOptions _serializerOptions;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
        _serializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var error = new ErrorResponse();
        context.Response.ContentType = "application/json";
        switch (exception)
        {
            case RequestValidationException _:
            case AlreadyExistException _:
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                error.Message = exception.Message;
                break;
            case NotFoundException _:
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                error.Message = exception.Message;
                break;
            case AuthenticationException _:
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                error.Message = Errors.AuthFailed;
                break;
            default:
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                error.Message = Errors.UnhandledException;
                break;
        }

        var result = JsonSerializer.Serialize(error, _serializerOptions);
        return context.Response.WriteAsync(result);
    }
}