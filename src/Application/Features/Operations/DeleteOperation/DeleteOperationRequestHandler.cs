using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.DeleteOperation;

internal sealed class DeleteOperationRequestHandler : IRequestHandler<DeleteOperationRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public DeleteOperationRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(DeleteOperationRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var operation = await _dbContext
            .Operations
            .Include(o => o.Account)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (operation is null) throw new NotFoundException(ErrorsOperations.NotFound);

        operation.Account.Balance -= operation.Amount;
        _dbContext.Operations.Remove(operation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}