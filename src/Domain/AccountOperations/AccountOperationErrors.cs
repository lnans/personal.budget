using ErrorOr;

namespace Domain.AccountOperations;

public static class AccountOperationErrors
{
    public static Error AccountOperationDescriptionRequired =>
        Error.Validation(
            code: "AccountOperation.Description.Required",
            description: "Account operation description is required."
        );

    public static Error AccountOperationDescriptionTooLong =>
        Error.Validation(
            code: "AccountOperation.Description.TooLong",
            description: $"Account operation description must not exceed {AccountOperationConstants.MaxDescriptionLength} characters."
        );
}
