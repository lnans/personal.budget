using FluentValidation;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountValidator()
    {
        RuleFor(q => q.Id).NotEmpty();
    }
}
