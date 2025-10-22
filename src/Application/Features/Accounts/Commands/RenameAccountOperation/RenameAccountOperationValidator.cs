using Application.Extensions;
using Domain.AccountOperations;
using FluentValidation;

namespace Application.Features.Accounts.Commands.RenameAccountOperation;

public class RenameAccountOperationValidator : AbstractValidator<RenameAccountOperationCommand>
{
    public RenameAccountOperationValidator()
    {
        RuleFor(q => q.Description)
            .NotEmpty()
            .WithError(AccountOperationErrors.AccountOperationDescriptionRequired)
            .MaximumLength(AccountOperationConstants.MaxDescriptionLength)
            .WithError(AccountOperationErrors.AccountOperationDescriptionTooLong);
    }
}
