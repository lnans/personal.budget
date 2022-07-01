using Domain;
using FluentValidation;

namespace Application.Features.Accounts.Commands.PatchAccount;

public class PatchAccountValidator : AbstractValidator<PatchAccountRequest>
{
    public PatchAccountValidator()
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