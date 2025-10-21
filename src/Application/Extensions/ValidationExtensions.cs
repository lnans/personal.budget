using ErrorOr;
using FluentValidation;

namespace Application.Extensions;

internal static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error
    ) => rule.WithMessage(error.Description).WithErrorCode(error.Code);
}
