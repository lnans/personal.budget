using Domain;
using FluentValidation;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public class DeleteAccountValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(Errors.AccountIdRequired);
    }
}