using Domain;
using FluentValidation;

namespace Application.Features.OperationTags.Commands.CreateOperationTag;

public class CreateOperationTagValidator : AbstractValidator<CreateOperationTagRequest>
{
    public CreateOperationTagValidator()
    {
        RuleFor(operationTag => operationTag.Name)
            .NotEmpty()
            .WithMessage(Errors.OperationTagNameRequired);

        RuleFor(operationTag => operationTag.Color)
            .Matches(RegexRules.ColorHex)
            .WithMessage(Errors.OperationTagColorInvalid);
    }
}