using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Transactions.Commands.PutTransaction;

public record PutTransactionRequest : IRequest<Unit>
{
    public string Id { get; set; }
    public string Description { get; init; }
    public string TagId { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}

public class PutTransactionCommandHandler : IRequestHandler<PutTransactionRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public PutTransactionCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(PutTransactionRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var transaction = await _dbContext
            .Transactions
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (transaction is null) throw new NotFoundException(Errors.TransactionNotFound);

        if (!string.IsNullOrWhiteSpace(request.TagId))
        {
            var tag = await _dbContext
                .Tags
                .FirstOrDefaultAsync(o => o.Id == request.TagId && o.OwnerId == userId, cancellationToken);

            if (tag is null) throw new NotFoundException(Errors.TagNotFound);

            transaction.Tag = tag;
        }
        else if (transaction.Tag is not null)
        {
            transaction.Tag = null;
        }

        // Update transaction Amount
        if (request.ExecutionDate.HasValue && transaction.ExecutionDate.HasValue)
            transaction.Account.Balance += request.Amount - transaction.Amount;
        // Execute transaction
        else if (request.ExecutionDate.HasValue && !transaction.ExecutionDate.HasValue)
            transaction.Account.Balance += request.Amount;
        // Cancel transaction
        else if (!request.ExecutionDate.HasValue && transaction.ExecutionDate.HasValue) transaction.Account.Balance -= transaction.Amount;

        transaction.Description = request.Description;
        transaction.Amount = request.Amount;
        transaction.CreationDate = request.CreationDate;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}