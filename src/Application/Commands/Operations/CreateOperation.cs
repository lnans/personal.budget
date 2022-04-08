namespace Application.Commands.Operations;

public record CreateOperationRequest(
    string Description,
    string AccountId,
    string TransferAccountId,
    string OperationTagId,
    OperationType OperationType,
    decimal Amount,
    DateTime OperationDate,
    DateTime? TransferDate) : IRequest<CreateOperationResponse>;

public record CreateOperationResponse(
    string Description,
    string AccountId,
    string AccountName,
    string TransferAccountId,
    string TransferAccountName,
    string OperationTagId,
    string OperationTagName,
    OperationType OperationType,
    decimal Amount,
    DateTime OperationDate,
    DateTime? TransferDate);

public class CreateOperationValidator : AbstractValidator<CreateOperationRequest>
{
    public CreateOperationValidator()
    {
        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage(Errors.OperationDescriptionRequired);
        RuleFor(p => p.AccountId)
            .NotEmpty()
            .WithMessage(Errors.OperationAccountRequired);
        RuleFor(p => p.TransferAccountId)
            .NotEmpty()
            .When(p => p.OperationType is OperationType.Transfer)
            .WithMessage(Errors.OperationTransferAccountRequired);
        RuleFor(p => p.OperationType)
            .IsInEnum()
            .WithMessage(Errors.OperationTypeUnknown);
        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(Errors.OperationAmountRequired);
        RuleFor(p => p.OperationDate)
            .NotEqual(default(DateTime))
            .WithMessage(Errors.OperationDateRequired);
        RuleFor(p => p.TransferDate)
            .NotNull()
            .When(p => p.OperationType is OperationType.Transfer)
            .WithMessage(Errors.OperationTransferDateRequired);
    }
}

public class CreateOperation : IRequestHandler<CreateOperationRequest, CreateOperationResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public CreateOperation(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<CreateOperationResponse> Handle(CreateOperationRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.OperationAccountNotFound);

        // Specific treatment for transfer operation
        if (request.OperationType is OperationType.Transfer)
        {
            var transferAccount = await _dbContext
                .Accounts
                .FirstOrDefaultAsync(a => a.Id == request.TransferAccountId && a.OwnerId == userId, cancellationToken);

            if (transferAccount is null) throw new NotFoundException(Errors.OperationTransferAccountNotFound);

            var transferToOperation = new Operation
            {
                Id = Guid.NewGuid().ToString(),
                Description = request.Description,
                Account = account,
                TransferAccount = transferAccount,
                Amount = request.Amount,
                Type = request.OperationType,
                CreatedById = userId,
                OperationDate = request.OperationDate
            };
            var transferFromOperation = new Operation
            {
                Id = Guid.NewGuid().ToString(),
                Description = request.Description,
                Account = transferAccount,
                TransferAccount = account,
                Amount = -request.Amount,
                Type = request.OperationType,
                CreatedById = userId,
                OperationDate = request.TransferDate.GetValueOrDefault() // already asserted by validator
            };

            // Update accounts balance
            account.Balance += request.Amount;
            transferAccount.Balance -= request.Amount;

            await _dbContext.Operations.AddAsync(transferToOperation, cancellationToken);
            await _dbContext.Operations.AddAsync(transferFromOperation, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new CreateOperationResponse(
                transferToOperation.Description,
                transferToOperation.Id,
                transferToOperation.Account.Name,
                transferToOperation.TransferAccount.Id,
                transferToOperation.TransferAccount.Name,
                null,
                null,
                transferToOperation.Type,
                transferToOperation.Amount,
                transferToOperation.OperationDate,
                transferFromOperation.OperationDate);
        }

        OperationTag tag = null;
        if (!string.IsNullOrWhiteSpace(request.OperationTagId))
        {
            tag = await _dbContext
                .OperationTags
                .FirstOrDefaultAsync(o => o.Id == request.OperationTagId && o.OwnerId == userId, cancellationToken);

            if (tag is null) throw new NotFoundException(Errors.OperationTagNotFound);
        }

        var operation = new Operation
        {
            Id = Guid.NewGuid().ToString(),
            Description = request.Description,
            Account = account,
            Tag = tag,
            Amount = request.Amount,
            Type = request.OperationType,
            OperationDate = request.OperationDate,
            CreatedById = userId
        };

        // Update account balance
        account.Balance += request.Amount;

        await _dbContext.Operations.AddAsync(operation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOperationResponse(
            operation.Description,
            operation.Account.Id,
            operation.Account.Name,
            null,
            null,
            operation.Tag?.Id,
            operation.Tag?.Name,
            operation.Type,
            operation.Amount,
            operation.OperationDate,
            null);
    }
}