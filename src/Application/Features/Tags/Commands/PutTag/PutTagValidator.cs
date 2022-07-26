using Domain;
using FluentValidation;

namespace Application.Features.Tags.Commands.PutTag;

public class PutTagValidator : AbstractValidator<PutTagRequest>
{
    public PutTagValidator()
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