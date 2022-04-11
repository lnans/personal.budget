namespace Application.Commands.Operations;

public record UpdateOperationRequest(string Description, string OperationTagId, decimal Amount);

public record UpdateOperationRequestWithId(string Id, UpdateOperationRequest Request) : IRequest<UpdateOperationResponse>;

public record UpdateOperationResponse(
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

public class UpdateOperationValidator : AbstractValidator<UpdateOperationRequestWithId>
{
    public UpdateOperationValidator()
    {
        RuleFor(p => p.Request.Description)
            .NotEmpty()
            .WithMessage(Errors.OperationDescriptionRequired);
        RuleFor(p => p.Request.Amount)
            .NotEqual(default(decimal))
            .WithMessage(Errors.OperationAmountRequired);
    }
}

public class UpdateOperation : IRequestHandler<UpdateOperationRequestWithId, UpdateOperationResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public UpdateOperation(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<UpdateOperationResponse> Handle(UpdateOperationRequestWithId request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var operation = await _dbContext
            .Operations
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (operation is null) throw new NotFoundException(Errors.OperationNotFound);

        if (!string.IsNullOrWhiteSpace(request.Request.OperationTagId))
        {
            var tag = await _dbContext
                .OperationTags
                .FirstOrDefaultAsync(o => o.Id == request.Request.OperationTagId && o.OwnerId == userId, cancellationToken);

            if (tag is null) throw new NotFoundException(Errors.OperationTagNotFound);

            operation.Tag = tag;
        }
        else if (operation.Tag is not null)
        {
            _dbContext.OperationTags.Remove(operation.Tag);
        }

        if (operation.ExecutionDate.HasValue) operation.Account.Balance += request.Request.Amount - operation.Amount;

        operation.Description = request.Request.Description;
        operation.Amount = request.Request.Amount;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOperationResponse(
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