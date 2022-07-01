using Domain;
using FluentValidation;

namespace Application.Features.Authentication.Commands.SignIn;

public class SignInValidator : AbstractValidator<SignInRequest>
{
    public SignInValidator()
    {
        RuleFor(c => c.Username)
            .NotEmpty()
            .WithMessage(Errors.AuthLoginRequired);

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage(Errors.AuthPasswordRequired);
    }
}