using Domain;
using FluentValidation;

namespace Application.Features.Accounts.Commands.ArchivedAccount;

public class ArchivedAccountValidator : AbstractValidator<ArchivedAccountRequest>
{
    public ArchivedAccountValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(Errors.AccountIdRequired);
    }
}