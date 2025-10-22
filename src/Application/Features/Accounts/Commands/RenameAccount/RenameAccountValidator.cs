using Application.Extensions;
using Domain.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.RenameAccount;

public class RenameAccountValidator : AbstractValidator<RenameAccountCommand>
{
    public RenameAccountValidator()
    {
        RuleFor(q => q.Name)
            .NotEmpty()
            .WithError(AccountErrors.AccountNameRequired)
            .MaximumLength(AccountConstants.MaxNameLength)
            .WithError(AccountErrors.AccountNameTooLong);
    }
}
