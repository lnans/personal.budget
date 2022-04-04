namespace Application.Commands.OperationTags;

public record DeleteOperationTagRequest(string Id) : IRequest<Unit>;

public class DeleteOperationTag : IRequestHandler<DeleteOperationTagRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public DeleteOperationTag(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(DeleteOperationTagRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var operationTag = await _dbContext
            .OperationTags
            .FirstOrDefaultAsync(op => op.Id == request.Id && op.OwnerId == userId, cancellationToken);

        if (operationTag is null) throw new NotFoundException(Errors.OperationTagNotFound);

        _dbContext.OperationTags.Remove(operationTag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}