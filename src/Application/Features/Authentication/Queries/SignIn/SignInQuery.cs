using ErrorOr;
using MediatR;

namespace Application.Features.Authentication.Queries.SignIn;

public sealed class SignInQuery : IRequest<ErrorOr<SignInResponse>>
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}
