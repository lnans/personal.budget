using Domain;
using FluentValidation;

namespace Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage(Errors.AccountNameRequired);

        RuleFor(p => p.Bank)
            .NotEmpty()
            .WithMessage(Errors.AccountBankRequired);

        RuleFor(p => p.Type)
            .IsInEnum()
            .WithMessage(Errors.AccountTypeUnknown);
    }
}