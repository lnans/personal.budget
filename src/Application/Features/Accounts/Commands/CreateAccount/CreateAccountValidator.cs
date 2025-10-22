using Application.Extensions;
using Domain.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(q => q.Name)
            .NotEmpty()
            .WithError(AccountErrors.AccountNameRequired)
            .MaximumLength(AccountConstants.MaxNameLength)
            .WithError(AccountErrors.AccountNameTooLong);
    }
}
