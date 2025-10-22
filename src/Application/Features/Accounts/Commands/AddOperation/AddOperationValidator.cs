using Application.Extensions;
using Domain.AccountOperations;
using FluentValidation;

namespace Application.Features.Accounts.Commands.AddOperation;

public class AddOperationValidator : AbstractValidator<AddOperationCommand>
{
    public AddOperationValidator()
    {
        RuleFor(q => q.Description)
            .NotEmpty()
            .WithError(AccountOperationErrors.AccountOperationDescriptionRequired)
            .MaximumLength(AccountOperationConstants.MaxDescriptionLength)
            .WithError(AccountOperationErrors.AccountOperationDescriptionTooLong);
    }
}
