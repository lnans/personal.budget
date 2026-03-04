using Application.Extensions;
using Domain.AccountOperations;
using FluentValidation;

namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

internal sealed class AddAccountOperationValidator : AbstractValidator<AddAccountOperationCommand>
{
    public AddAccountOperationValidator()
    {
        RuleFor(q => q.Description)
            .NotEmpty()
            .WithError(AccountOperationErrors.AccountOperationDescriptionRequired)
            .MaximumLength(AccountOperationConstants.MaxDescriptionLength)
            .WithError(AccountOperationErrors.AccountOperationDescriptionTooLong);
    }
}
