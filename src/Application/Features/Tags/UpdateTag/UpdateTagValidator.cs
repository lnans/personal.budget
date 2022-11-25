using Application.Errors;
using FluentValidation;

namespace Application.Features.Tags.UpdateTag;

public class UpdateTagValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagValidator()
    {
        RuleFor(tag => tag.Name)
            .NotEmpty()
            .WithMessage(ErrorsTags.NameRequired);

        RuleFor(tag => tag.Color)
            .Matches(@"^#(?:[0-9a-fA-F]{3}){1,2}$")
            .WithMessage(ErrorsTags.ColorInvalid);
    }
}