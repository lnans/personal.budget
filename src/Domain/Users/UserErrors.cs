using ErrorOr;

namespace Domain.Users;

public static class UserErrors
{
    public static Error UserLoginRequired =>
        Error.Validation(code: "User.Login.Required", description: "User login is required.");

    public static Error UserLoginTooLong =>
        Error.Validation(
            code: "User.Login.TooLong",
            description: $"User login must not exceed {UserConstants.MaxLoginLength} characters."
        );

    public static Error UserPasswordRequired =>
        Error.Validation(code: "User.Password.Required", description: "User password is required.");

    public static Error UserRefreshTokenRequired =>
        Error.Validation(code: "User.RefreshToken.Required", description: "User refresh token is required.");

    public static Error UserInvalidCredentials =>
        Error.Unauthorized(code: "User.InvalidCredentials", description: "Invalid user credentials.");

    public static Error UserInvalidRefreshToken =>
        Error.Unauthorized(code: "User.InvalidRefreshToken", description: "Invalid or expired refresh token.");

    public static Error UserNotFound => Error.NotFound(code: "User.NotFound", description: "User not found.");
}
