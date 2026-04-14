using Application.Extensions;
using Domain;
using Domain.Tags;
using FluentValidation;

namespace Application.Features.Tags.Commands.UpdateTag;

internal sealed class UpdateTagValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagValidator()
    {
        RuleFor(q => q.Id).NotEmpty();

        RuleFor(q => q.Name)
            .NotEmpty()
            .WithError(TagErrors.TagNameRequired)
            .MaximumLength(TagConstants.MaxNameLength)
            .WithError(TagErrors.TagNameTooLong);

        RuleFor(q => q.Color)
            .NotEmpty()
            .WithError(TagErrors.TagColorRequired)
            .Matches(Regexes.HexColorRegex)
            .WithError(TagErrors.TagColorInvalid);
    }
}
