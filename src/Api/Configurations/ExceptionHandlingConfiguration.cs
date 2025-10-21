using Api.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Configurations;

public static class ExceptionHandlingConfiguration
{
    public static void UseExceptionHandling(this WebApplication app) =>
        app.UseExceptionHandler(errorApp =>
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = Problems.ProblemContentType;

                var problem = Problems.InternalServerError(exception, context);
                await context.Response.WriteAsJsonAsync(problem);
            })
        );
}
