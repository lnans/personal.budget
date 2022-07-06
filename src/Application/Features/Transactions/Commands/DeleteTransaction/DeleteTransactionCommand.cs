using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Transactions.Commands.DeleteTransaction;

public record DeleteTransactionRequest : IRequest<Unit>
{
    public string Id { get; init; }
}

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public DeleteTransactionCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(DeleteTransactionRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var transaction = await _dbContext
            .Transactions
            .Include(o => o.Account)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (transaction is null) throw new NotFoundException(Errors.TransactionNotFound);

        transaction.Account.Balance -= transaction.Amount;
        _dbContext.Transactions.Remove(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}