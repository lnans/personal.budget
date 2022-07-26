using Application.Features.Authentication.Commands.SignIn;
using Application.Features.Authentication.Queries.GetAuthInfo;

namespace Api.EndPoints;

public class AuthenticationEndPoints : IEndPoints
{
    private const string Tag = "Authentication";

    public void Register(WebApplication app)
    {
        app.MapPost("auth/signin", SignIn)
            .Summary("Sign In", "Sign in with user credentials")
            .Response<AuthenticationDto>(200, "User was successfully authenticated")
            .Response<ErrorResponse>(401, "Wrong user credentials")
            .WithTags(Tag)
            .AllowAnonymous();

        app.MapGet("auth/me", GetInfo)
            .Summary("Get auth info", "Return current logged user information")
            .Response<AuthenticationInfoDto>(200, "User information")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);
    }

    private static async Task<IResult> SignIn(IMediator mediator, SignInRequest request, CancellationToken ct) => Results.Ok(await mediator.Send(request, ct));
    private static async Task<IResult> GetInfo(IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(new GetAuthInfoRequest(), ct));
}