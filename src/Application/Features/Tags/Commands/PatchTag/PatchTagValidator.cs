using Domain;
using FluentValidation;

namespace Application.Features.Tags.Commands.PatchTag;

public class PatchTagValidator : AbstractValidator<PatchTagRequest>
{
    public PatchTagValidator()
    {
        RuleFor(tag => tag.Id)
            .NotEmpty()
            .WithMessage(Errors.TagIdRequired);

        RuleFor(tag => tag.Name)
            .NotEmpty()
            .WithMessage(Errors.TagNameRequired);

        RuleFor(tag => tag.Color)
            .Matches(RegexRules.ColorHex)
            .WithMessage(Errors.TagColorInvalid);
    }
}