using System.Net.Mime;
using Api.Extensions;
using ErrorOr;
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
                context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

                var error = Error.Unexpected("Error.Unexpected", exception?.Message ?? "An unexpected error occurred.");
                var problem = error.ToProblem(context, exception);
                await context.Response.WriteAsJsonAsync(problem);
            })
        );
}
