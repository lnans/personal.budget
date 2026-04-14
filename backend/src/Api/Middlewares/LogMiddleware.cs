using System.Diagnostics;
using Serilog.Context;

namespace Api.Middlewares;

public class LogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogMiddleware> _logger;

    public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var _ = LogContext.PushProperty("RequestId", context.TraceIdentifier);

        var request = context.Request;
        var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

        _logger.LogInformation("Starting HTTP {Method} {Path}{QueryString}", request.Method, request.Path, query);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Finished HTTP {Method} {Path} {ContentType} Status: {StatusCode} in {ElapsedMilliseconds}ms",
                request.Method,
                request.Path,
                context.Response.ContentType,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}
