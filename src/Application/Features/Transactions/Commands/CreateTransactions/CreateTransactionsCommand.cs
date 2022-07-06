using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Transactions.Commands.CreateTransactions;

public record CreateTransactionsRequest : IRequest<Unit>
{
    public string AccountId { get; init; }
    public CreateTransactionDto[] Transactions { get; init; }
}

public class CreateTransactionsCommandHandler : IRequestHandler<CreateTransactionsRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public CreateTransactionsCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(CreateTransactionsRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        var transactions = new List<Transaction>();
        foreach (var transactionDto in request.Transactions)
        {
            var transaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Description = transactionDto.Description,
                Account = account,
                Amount = transactionDto.Amount,
                Type = transactionDto.Type,
                CreationDate = transactionDto.CreationDate,
                ExecutionDate = transactionDto.ExecutionDate,
                CreatedById = userId
            };

            if (!string.IsNullOrWhiteSpace(transactionDto.TagId))
            {
                var tag = await _dbContext
                    .Tags
                    .FirstOrDefaultAsync(o => o.Id == transactionDto.TagId && o.OwnerId == userId, cancellationToken);

                if (tag is null) throw new NotFoundException(Errors.TagNotFound);

                transaction.Tag = tag;
            }

            // Update account balance
            if (transactionDto.ExecutionDate.HasValue) account.Balance += transactionDto.Amount;

            transactions.Add(transaction);
        }

        await _dbContext.Transactions.AddRangeAsync(transactions, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}