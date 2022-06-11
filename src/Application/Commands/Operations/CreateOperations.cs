using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Operations;

public record CreateOperation(
    string Description,
    string OperationTagId,
    OperationType OperationType,
    decimal Amount,
    DateTime CreationDate,
    DateTime? ExecutionDate);

public record CreateOperationsRequest(string AccountId, CreateOperation[] Operations) : IRequest<Unit>;

public class CreateOperationValidator : AbstractValidator<CreateOperation>
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

        RuleForEach(p => p.Operations).SetValidator((new CreateOperationValidator()));
    }
}

public class CreateOperations : IRequestHandler<CreateOperationsRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public CreateOperations(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(CreateOperationsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        var operations = new List<Operation>();
        foreach (var requestOperation in request.Operations)
        {
            var operation = new Operation
            {
                Id = Guid.NewGuid().ToString(),
                Description = requestOperation.Description,
                Account = account,
                Amount = requestOperation.Amount,
                Type = requestOperation.OperationType,
                CreationDate = requestOperation.CreationDate,
                ExecutionDate = requestOperation.ExecutionDate,
                CreatedById = userId
            };

            if (!string.IsNullOrWhiteSpace(requestOperation.OperationTagId))
            {
                var tag = await _dbContext
                    .OperationTags
                    .FirstOrDefaultAsync(o => o.Id == requestOperation.OperationTagId && o.OwnerId == userId, cancellationToken);

                if (tag is null) throw new NotFoundException(Errors.OperationTagNotFound);

                operation.Tag = tag;
            }

            // Update account balance
            if (requestOperation.ExecutionDate.HasValue) account.Balance += requestOperation.Amount;
            
            operations.Add(operation);
        }

        await _dbContext.Operations.AddRangeAsync(operations, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}