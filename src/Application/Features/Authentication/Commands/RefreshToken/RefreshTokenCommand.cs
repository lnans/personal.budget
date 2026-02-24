using Application.Interfaces;

namespace Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommand : ICommand<RefreshTokenResponse>
{
    public required string RefreshToken { get; set; }
}
