using Application.Errors;
using FluentValidation;

namespace Application.Features.Accounts.UpdateAccount;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage(ErrorsAccounts.NameRequired);

        RuleFor(p => p.Bank)
            .NotEmpty()
            .WithMessage(ErrorsAccounts.BankRequired);
    }
}