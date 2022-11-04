namespace Personal.Budget.Api.Features.Authentication.SignIn;

public sealed class SignInRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}