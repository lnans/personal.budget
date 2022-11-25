using Application.Errors;
using FluentValidation;

namespace Application.Features.Accounts.CreateAccount;

public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage(ErrorsAccounts.NameRequired);

        RuleFor(p => p.Bank)
            .NotEmpty()
            .WithMessage(ErrorsAccounts.BankRequired);

        RuleFor(p => p.Type)
            .IsInEnum()
            .WithMessage(ErrorsAccounts.TypeUnknown);
    }
}