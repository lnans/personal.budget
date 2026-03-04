using Application.Extensions;
using Domain.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.CreateAccount;

internal sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
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

        RuleFor(q => q.Type).IsInEnum().WithError(AccountErrors.AccountTypeUnknown);
    }
}
