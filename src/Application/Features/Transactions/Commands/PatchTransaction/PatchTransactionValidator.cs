using Domain;
using FluentValidation;

namespace Application.Features.Transactions.Commands.PatchTransaction;

public class PatchTransactionValidator : AbstractValidator<PatchTransactionRequest>
{
    public PatchTransactionValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(Errors.TransactionRequired);

        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage(Errors.TransactionDescriptionRequired);

        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(Errors.TransactionAmountRequired);

        RuleFor(p => p.CreationDate)
            .NotEqual(default(DateTime))
            .WithMessage(Errors.TransactionCreationDateRequired);
    }
}