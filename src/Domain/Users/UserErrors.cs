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

    public static Error UserPasswordHashRequired =>
        Error.Validation(code: "User.PasswordHash.Required", description: "User password hash is required.");

    public static Error UserInvalidCredentials =>
        Error.Validation(code: "User.InvalidCredentials", description: "Invalid user credentials.");
}
