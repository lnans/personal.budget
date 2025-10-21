using Application.Extensions;
using Domain.Users;
using FluentValidation;

namespace Application.Features.Authentication.Queries.SignIn;

public class SignInValidator : AbstractValidator<SignInQuery>
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
