using FluentValidation;

namespace Application.Features.Accounts.Commands.DeleteAccount;

internal sealed class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountValidator()
    {
        RuleFor(q => q.Id).NotEmpty();
    }
}
