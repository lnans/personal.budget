using Application.Extensions;
using Domain.AccountOperations;
using FluentValidation;

namespace Application.Features.Accounts.Commands.AddAccountOperation;

public class AddAccountOperationValidator : AbstractValidator<AddAccountOperationCommand>
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
