using Application.Interfaces;

namespace Application.Features.Authentication.Commands.SignIn;

public sealed class SignInCommand : ICommand<SignInResponse>
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}
