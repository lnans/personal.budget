using Domain;
using FluentValidation;

namespace Application.Features.OperationTags.Commands.PatchOperationTag;

public class PatchOperationTagValidator : AbstractValidator<PatchOperationTagRequest>
{
    public PatchOperationTagValidator()
    {
        RuleFor(operationTag => operationTag.Id)
            .NotEmpty()
            .WithMessage(Errors.OperationTagIdRequired);

        RuleFor(operationTag => operationTag.Name)
            .NotEmpty()
            .WithMessage(Errors.OperationTagNameRequired);

        RuleFor(operationTag => operationTag.Color)
            .Matches(RegexRules.ColorHex)
            .WithMessage(Errors.OperationTagColorInvalid);
    }
}