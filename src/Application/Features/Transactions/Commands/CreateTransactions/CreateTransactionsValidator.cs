using Domain;
using FluentValidation;

namespace Application.Features.Transactions.Commands.CreateTransactions;

public class CreateTransactionsValidator : AbstractValidator<CreateTransactionsRequest>
{
    public CreateTransactionsValidator()
    {
        RuleFor(p => p.AccountId)
            .NotEmpty()
            .WithMessage(Errors.TransactionAccountRequired);

        RuleFor(p => p.Transactions)
            .NotEmpty()
            .WithMessage(Errors.TransactionRequired);

        RuleForEach(p => p.Transactions).SetValidator(new CreateTransactionValidator());
    }
}

public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionValidator()
    {
        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage(Errors.TransactionDescriptionRequired);
        RuleFor(p => p.Type)
            .IsInEnum()
            .WithMessage(Errors.TransactionTypeUnknown);
        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(Errors.TransactionAmountRequired);
        RuleFor(p => p.CreationDate)
            .NotEqual(default(DateTime))
            .WithMessage(Errors.TransactionCreationDateRequired);
    }
}