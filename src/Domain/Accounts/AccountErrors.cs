using ErrorOr;

namespace Domain.Accounts;

public static class AccountErrors
{
    public static Error AccountNameRequired =>
        Error.Validation(code: "Account.Name.Required", description: "Account name is required.");

    public static Error AccountNameTooLong =>
        Error.Validation(
            code: "Account.Name.TooLong",
            description: $"Account name must not exceed {AccountConstants.MaxNameLength} characters."
        );

    public static Error AccountBankRequired =>
        Error.Validation(code: "Account.Bank.Required", description: "Account bank is required.");

    public static Error AccountBankTooLong =>
        Error.Validation(
            code: "Account.Bank.TooLong",
            description: $"Account bank must not exceed {AccountConstants.MaxBankLength} characters."
        );

    public static Error AccountTypeUnknown =>
        Error.Validation(code: "Account.AccountType.Unknown", description: "Account type is not valid.");

    public static Error AccountNotFound => Error.NotFound(code: "Account.NotFound", description: "Account not found.");

    public static Error AccountAlreadyDeleted =>
        Error.Validation(code: "Account.AlreadyDeleted", description: "Account is already deleted.");

    public static Error AccountIdInvalid =>
        Error.Validation(code: "Account.Id.Invalid", description: "Account ID must be a valid non-empty identifier.");
}
