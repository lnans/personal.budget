using ErrorOr;
using MediatR;

namespace Application.Features.Authentication.Commands.SignIn;

public sealed class SignInCommand : IRequest<ErrorOr<SignInResponse>>
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}
