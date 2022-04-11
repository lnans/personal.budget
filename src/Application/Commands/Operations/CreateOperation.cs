namespace Application.Commands.Operations;

public record CreateOperationRequest(
    string Description,
    string AccountId,
    string OperationTagId,
    OperationType OperationType,
    decimal Amount,
    DateTime CreationDate,
    DateTime? ExecutionDate) : IRequest<CreateOperationResponse>;

public record CreateOperationResponse(
    string Id,
    string Description,
    string AccountId,
    string AccountName,
    string TagName,
    string TagColor,
    OperationType OperationType,
    decimal Amount,
    DateTime CreationDate,
    DateTime? ExecutionDate);

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
        RuleFor(p => p.OperationType)
            .IsInEnum()
            .WithMessage(Errors.OperationTypeUnknown);
        RuleFor(p => p.Amount)
            .NotEqual(default(decimal))
            .WithMessage(Errors.OperationAmountRequired);
        RuleFor(p => p.CreationDate)
            .NotEqual(default(DateTime))
            .WithMessage(Errors.OperationDateRequired);
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

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        var operation = new Operation
        {
            Id = Guid.NewGuid().ToString(),
            Description = request.Description,
            Account = account,
            Amount = request.Amount,
            Type = request.OperationType,
            CreationDate = request.CreationDate,
            ExecutionDate = request.ExecutionDate,
            CreatedById = userId
        };

        if (!string.IsNullOrWhiteSpace(request.OperationTagId))
        {
            var tag = await _dbContext
                .OperationTags
                .FirstOrDefaultAsync(o => o.Id == request.OperationTagId && o.OwnerId == userId, cancellationToken);

            if (tag is null) throw new NotFoundException(Errors.OperationTagNotFound);

            operation.Tag = tag;
        }

        // Update account balance
        if(request.ExecutionDate.HasValue) account.Balance += request.Amount;

        await _dbContext.Operations.AddAsync(operation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOperationResponse(
            operation.Id,
            operation.Description,
            operation.Account.Id,
            operation.Account.Name,
            operation.Tag?.Name,
            operation.Tag?.Color,
            operation.Type,
            operation.Amount,
            operation.CreationDate,
            operation.ExecutionDate);
    }
}