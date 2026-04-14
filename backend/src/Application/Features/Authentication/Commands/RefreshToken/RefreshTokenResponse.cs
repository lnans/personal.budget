namespace Application.Features.Authentication.Commands.RefreshToken;

public record RefreshTokenResponse(string Bearer, string RefreshToken) { }
