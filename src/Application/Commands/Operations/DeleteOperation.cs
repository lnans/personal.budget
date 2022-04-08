namespace Application.Commands.Operations;

public record DeleteOperationRequest(string Id) : IRequest<Unit>;

public class DeleteOperation : IRequestHandler<DeleteOperationRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public DeleteOperation(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(DeleteOperationRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var operation = await _dbContext
            .Operations
            .Include(o => o.Account)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (operation is null) throw new NotFoundException(Errors.OperationNotFound);

        operation.Account.Balance -= operation.Amount;
        _dbContext.Operations.Remove(operation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}