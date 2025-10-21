using Application.Extensions;
using Domain.Users;
using FluentValidation;

namespace Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithError(UserErrors.UserRefreshTokenRequired);
    }
}
