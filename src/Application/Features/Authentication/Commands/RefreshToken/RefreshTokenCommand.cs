using ErrorOr;
using MediatR;

namespace Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommand : IRequest<ErrorOr<RefreshTokenResponse>>
{
    public required string RefreshToken { get; set; }
}
