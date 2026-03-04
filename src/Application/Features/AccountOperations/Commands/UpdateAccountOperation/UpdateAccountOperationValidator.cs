using Application.Extensions;
using Domain.AccountOperations;
using FluentValidation;

namespace Application.Features.AccountOperations.Commands.UpdateAccountOperation;

internal sealed class UpdateAccountOperationValidator : AbstractValidator<UpdateAccountOperationCommand>
{
    public UpdateAccountOperationValidator()
    {
        RuleFor(q => q.Description)
            .NotEmpty()
            .WithError(AccountOperationErrors.AccountOperationDescriptionRequired)
            .MaximumLength(AccountOperationConstants.MaxDescriptionLength)
            .WithError(AccountOperationErrors.AccountOperationDescriptionTooLong);
    }
}
