using Ardalis.GuardClauses;

namespace Api.Authentication;

public static class AuthOptionsExtensions
{
    public static AuthTokenOptions GetAuthTokenOptions(this IConfiguration configuration)
    {
        var authOptions = configuration.GetSection("Auth").Get<AuthTokenOptions>();
        Guard.Against.Null(authOptions, nameof(authOptions), "'Auth' section is not found in the configuration.");
        Guard.Against.NullOrEmpty(
            authOptions.SecretKey,
            nameof(authOptions.SecretKey),
            "'Auth:SecretKey' is not found in the configuration."
        );
        Guard.Against.NullOrEmpty(
            authOptions.Issuer,
            nameof(authOptions.Issuer),
            "'Auth:Issuer' is not found in the configuration."
        );
        Guard.Against.NullOrEmpty(
            authOptions.Audience,
            nameof(authOptions.Audience),
            "'Auth:Audience' is not found in the configuration."
        );
        Guard.Against.Zero(
            authOptions.ExpirationMinutes,
            nameof(authOptions.ExpirationMinutes),
            "'Auth:ExpirationMinutes' is not found in the configuration."
        );
        Guard.Against.StringTooShort(
            authOptions.SecretKey,
            32,
            nameof(authOptions.SecretKey),
            "'Auth:SecretKey' must be at least 32 characters long."
        );
        return authOptions;
    }
}
