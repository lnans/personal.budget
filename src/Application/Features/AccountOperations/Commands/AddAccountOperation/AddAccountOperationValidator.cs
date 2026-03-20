using Application.Extensions;
using Domain.AccountOperations;
using Domain.Tags;
using FluentValidation;

namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

internal sealed class AddAccountOperationValidator : AbstractValidator<AddAccountOperationCommand>
{
    public AddAccountOperationValidator(TimeProvider timeProvider)
    {
        RuleFor(q => q.Description)
            .NotEmpty()
            .WithError(AccountOperationErrors.AccountOperationDescriptionRequired)
            .MaximumLength(AccountOperationConstants.MaxDescriptionLength)
            .WithError(AccountOperationErrors.AccountOperationDescriptionTooLong);

        RuleFor(q => q.OperationDate)
            .LessThanOrEqualTo(_ => timeProvider.GetUtcNow())
            .When(q => q.OperationDate.HasValue)
            .WithError(AccountOperationErrors.AccountOperationDateInFuture);

        When(
            q => q.TagIds is not null,
            () =>
                RuleFor(q => q.TagIds!)
                    .Must(tags => tags.Distinct().Count() == tags.Count)
                    .WithError(TagErrors.TagDuplicated)
        );
    }
}
