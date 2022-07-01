using Domain;
using FluentValidation;

namespace Application.Features.Operations.Commands.CreateOperations;

public class CreateOperationsValidator : AbstractValidator<CreateOperationsRequest>
{
    public CreateOperationsValidator()
    {
        RuleFor(p => p.AccountId)
            .NotEmpty()
            .WithMessage(Errors.OperationAccountRequired);

        RuleFor(p => p.Operations)
            .NotEmpty()
            .WithMessage(Errors.OperationRequired);

        RuleForEach(p => p.Operations).SetValidator(new CreateOperationValidator());
    }
}

public class CreateOperationValidator : AbstractValidator<CreateOperationDto>
{
    public CreateOperationValidator()
    {
        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage(Errors.OperationDescriptionRequired);
        RuleFor(p => p.OperationType)
            .IsInEnum()
            .WithMessage(Errors.OperationTypeUnknown);
        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(Errors.OperationAmountRequired);
        RuleFor(p => p.CreationDate)
            .NotEqual(default(DateTime))
            .WithMessage(Errors.OperationCreationDateRequired);
    }
}