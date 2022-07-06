using Domain;
using FluentValidation;

namespace Application.Features.Tags.Commands.DeleteTag;

public class DeleteTagValidator : AbstractValidator<DeleteTagRequest>
{
    public DeleteTagValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(Errors.TagIdRequired);
    }
}