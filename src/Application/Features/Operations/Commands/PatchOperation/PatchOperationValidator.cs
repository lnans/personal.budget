using Domain;
using FluentValidation;

namespace Application.Features.Operations.Commands.PatchOperation;

public class PatchOperationValidator : AbstractValidator<PatchOperationRequest>
{
    public PatchOperationValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(Errors.OperationRequired);

        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage(Errors.OperationDescriptionRequired);

        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(Errors.OperationAmountRequired);

        RuleFor(p => p.CreationDate)
            .NotEqual(default(DateTime))
            .WithMessage(Errors.OperationCreationDateRequired);
    }
}