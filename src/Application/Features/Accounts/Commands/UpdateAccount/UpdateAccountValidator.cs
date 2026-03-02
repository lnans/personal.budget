using Application.Extensions;
using Domain.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(q => q.Name)
            .NotEmpty()
            .WithError(AccountErrors.AccountNameRequired)
            .MaximumLength(AccountConstants.MaxNameLength)
            .WithError(AccountErrors.AccountNameTooLong);

        RuleFor(q => q.Bank)
            .NotEmpty()
            .WithError(AccountErrors.AccountBankRequired)
            .MaximumLength(AccountConstants.MaxBankLength)
            .WithError(AccountErrors.AccountBankTooLong);
    }
}
