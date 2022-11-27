using Application.Errors;
using FluentValidation;

namespace Application.Features.Operations.CreateOperations;

public class CreateOperationsValidator : AbstractValidator<CreateOperationsRequest>
{
    public CreateOperationsValidator()
    {
        RuleFor(p => p.Operations)
            .NotEmpty()
            .WithMessage(ErrorsOperations.Required);

        RuleForEach(p => p.Operations).SetValidator(new CreateOperationDataValidator());
    }
}

public class CreateOperationDataValidator : AbstractValidator<CreateOperationData>
{
    public CreateOperationDataValidator()
    {
        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage(ErrorsOperations.DescriptionRequired);
        RuleFor(p => p.Type)
            .IsInEnum()
            .WithMessage(ErrorsOperations.TypeUnknown);
        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(ErrorsOperations.AmountRequired);
        RuleFor(p => p.CreationDate)
            .NotEqual(default(DateTime))
            .WithMessage(ErrorsOperations.CreationDateRequired);
    }
}