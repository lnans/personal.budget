using Domain;
using FluentValidation;

namespace Application.Features.Accounts.Commands.PutAccount;

public class PutAccountValidator : AbstractValidator<PutAccountRequest>
{
    public PutAccountValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(Errors.AccountIdRequired);

        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage(Errors.AccountNameRequired);

        RuleFor(p => p.Bank)
            .NotEmpty()
            .WithMessage(Errors.AccountBankRequired);
    }
}