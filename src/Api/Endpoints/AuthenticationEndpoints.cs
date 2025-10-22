using Api.Configurations;
using Api.Extensions;
using Application.Features.Authentication.Commands.RefreshToken;
using Application.Features.Authentication.Commands.SignIn;
using Application.Features.Authentication.Queries.GetCurrentUser;
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
            .MapGet("", GetCurrentUser)
            .WithDescription("Get the authenticated user details")
            .WithSummary("Get current user")
            .Produces<GetCurrentUserResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(GetCurrentUser))
            .WithTags(Tag);

        group
            .MapPost("/signin", SignIn)
            .WithDescription("Sign in and get an authentication token")
            .WithSummary("Sign in")
            .Produces<SignInResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(SignIn))
            .WithTags(Tag)
            .AllowAnonymous();

        group
            .MapPost("/refresh", RefreshToken)
            .WithDescription("Refresh an access token using a refresh token")
            .WithSummary("Refresh token")
            .Produces<RefreshTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(RefreshToken))
            .WithTags(Tag)
            .AllowAnonymous();
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> SignIn(
        HttpContext context,
        IMediator mediator,
        [FromBody] SignInCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> RefreshToken(
        HttpContext context,
        IMediator mediator,
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }
}
