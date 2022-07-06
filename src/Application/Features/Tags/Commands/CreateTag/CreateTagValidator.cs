using Domain;
using FluentValidation;

namespace Application.Features.Tags.Commands.CreateTag;

public class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator()
    {
        RuleFor(tag => tag.Name)
            .NotEmpty()
            .WithMessage(Errors.TagNameRequired);

        RuleFor(tag => tag.Color)
            .Matches(RegexRules.ColorHex)
            .WithMessage(Errors.TagColorInvalid);
    }
}