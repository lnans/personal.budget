using Application.Extensions;
using Domain;
using Domain.Tags;
using FluentValidation;

namespace Application.Features.Tags.Commands.CreateTag;

internal sealed class CreateTagValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagValidator()
    {
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
