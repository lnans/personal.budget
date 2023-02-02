using Application.Errors;
using FluentValidation;

namespace Application.Features.Operations.UpdateOperation;

public sealed class UpdateOperationValidator : AbstractValidator<UpdateOperationRequest>
{
    public UpdateOperationValidator()
    {
        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage(ErrorsOperations.DescriptionRequired);

        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(ErrorsOperations.Required);

        RuleFor(p => p.CreationDate)
            .NotEqual(default(DateTime))
            .WithMessage(ErrorsOperations.CreationDateRequired);
    }
}