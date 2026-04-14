using ErrorOr;

namespace Application.Models.Pagination;

public static class PaginationErrors
{
    public static Error PageNumberInvalid =>
        Error.Validation(code: "Pagination.PageNumber.Invalid", description: "Page number must be greater than 0.");

    public static Error PageSizeInvalid =>
        Error.Validation(code: "Pagination.PageSize.Invalid", description: "Page size must be greater than 0.");

    public static Error PageSizeTooLarge =>
        Error.Validation(
            code: "Pagination.PageSize.TooLarge",
            description: $"Page size must not exceed {PaginationConstants.MaxPageSize}."
        );

    public static Error PageNumberTooLarge =>
        Error.Validation(
            code: "Pagination.PageNumber.TooLarge",
            description: "Page number is too large for the requested page size."
        );
}
