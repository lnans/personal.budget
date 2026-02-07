using ErrorOr;

namespace Application.Models;

public static class PaginationErrors
{
    public static Error PageNumberInvalid =>
        Error.Validation(code: "Pagination.PageNumber.Invalid", description: "Page number must be greater than 0.");

    public static Error PageSizeTooLarge =>
        Error.Validation(
            code: "Pagination.PageSize.TooLarge",
            description: $"Page size must not exceed {PaginationConstants.MaxPageSize}."
        );
}
