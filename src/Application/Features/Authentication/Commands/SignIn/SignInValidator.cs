using Application.Extensions;
using Domain.Users;
using FluentValidation;

namespace Application.Features.Authentication.Commands.SignIn;

public class SignInValidator : AbstractValidator<SignInCommand>
{
    public SignInValidator()
    {
        RuleFor(q => q.Login)
            .NotEmpty()
            .WithError(UserErrors.UserLoginRequired)
            .MaximumLength(UserConstants.MaxLoginLength)
            .WithError(UserErrors.UserLoginTooLong);

        RuleFor(q => q.Password).NotEmpty().WithError(UserErrors.UserPasswordRequired);
    }
}
