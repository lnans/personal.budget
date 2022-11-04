using FluentValidation;

namespace Personal.Budget.Api.Features.Authentication.SignIn;

public class SignInValidator : Validator<SignInRequest>
{
    public SignInValidator()
    {
        RuleFor(r => r.Username)
            .NotEmpty()
            .WithMessage("username required");

        RuleFor(r => r.Password)
            .NotEmpty()
            .WithMessage("password required");
    }
}