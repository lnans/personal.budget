using ErrorOr;

namespace Domain.Users;

public sealed class User : Entity
{
    private User(string login, string passwordHash, DateTimeOffset createdAt)
        : base(createdAt)
    {
        Login = login;
        PasswordHash = passwordHash;
    }

    public string Login { get; private set; }
    public string PasswordHash { get; private set; }

    public static ErrorOr<User> Create(string login, string passwordHash, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return UserErrors.UserLoginRequired;
        }

        if (login.Length > UserConstants.MaxLoginLength)
        {
            return UserErrors.UserLoginTooLong;
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return UserErrors.UserPasswordHashRequired;
        }

        return new User(login, passwordHash, createdAt);
    }

    public ErrorOr<Success> VerifyPassword(string passwordHashToVerify, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrEmpty(passwordHashToVerify) || !passwordHasher.Verify(passwordHashToVerify, PasswordHash))
        {
            return UserErrors.UserInvalidCredentials;
        }

        return Result.Success;
    }
}
