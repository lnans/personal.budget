using Domain;
using FluentValidation;

namespace Application.Features.OperationTags.Commands.DeleteOperationTag;

public class DeleteOperationTagValidator : AbstractValidator<DeleteOperationTagRequest>
{
    public DeleteOperationTagValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(Errors.OperationTagIdRequired);
    }
}