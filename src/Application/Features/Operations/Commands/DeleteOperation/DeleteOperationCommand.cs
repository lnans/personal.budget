using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.Commands.DeleteOperation;

public record DeleteOperationRequest : IRequest<Unit>
{
    public string Id { get; init; }
}

public class DeleteOperationCommandHandler : IRequestHandler<DeleteOperationRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public DeleteOperationCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(DeleteOperationRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
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