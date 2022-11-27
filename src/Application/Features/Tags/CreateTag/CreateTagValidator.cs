using Application.Errors;
using FluentValidation;

namespace Application.Features.Tags.CreateTag;

public class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator()
    {
        RuleFor(tag => tag.Name)
            .NotEmpty()
            .WithMessage(ErrorsTags.NameRequired);

        RuleFor(tag => tag.Color)
            .Matches(@"^#(?:[0-9a-fA-F]{3}){1,2}$") // match hexadecimal colors
            .WithMessage(ErrorsTags.ColorInvalid);
    }
}