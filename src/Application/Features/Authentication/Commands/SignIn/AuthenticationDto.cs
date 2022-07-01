namespace Application.Features.Authentication.Commands.SignIn;

public record AuthenticationDto
{
    public string Username { get; init; }
    public string Token { get; init; }
}