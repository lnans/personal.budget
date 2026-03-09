using ErrorOr;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Extensions;

internal static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error
    ) => rule.WithMessage(error.Description).WithErrorCode(error.Code);

    public static Error CreateValidationError(this List<ValidationFailure> failures)
    {
        var metadata = failures
            .GroupBy(failure => failure.ErrorCode)
            .ToDictionary(grouping => grouping.Key, object (grouping) => grouping.First().ErrorMessage);

        return Error.Validation(
            description: "Validation errors occurred.",
            code: "ValidationError",
            metadata: metadata
        );
    }
}
