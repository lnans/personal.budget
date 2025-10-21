using Api.Configurations;
using Api.Errors;
using Application.Features.Authentication.Queries.SignIn;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class AuthenticationEndpoints : IEndPoints
{
    private const string Tag = "Authentication";

    public void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/auth").RequireAuthorization();

        group
            .MapPost("/sign-in", SignIn)
            .WithDescription("Sign in and get an authentication token")
            .WithSummary("Sign in")
            .Produces<SignInResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(SignIn))
            .WithTags(Tag)
            .AllowAnonymous();
    }

    private static async Task<IResult> SignIn(
        HttpContext context,
        IMediator mediator,
        [FromBody] SignInQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(query, cancellationToken);
        return result.MatchFirst(Results.Ok, error => Results.Problem(error.ToProblemDetails(context)));
    }
}
