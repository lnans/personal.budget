using Application.Common.Interfaces;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Transactions.Queries.GetPaginatedTransactions;

public record GetPaginatedTransactionsRequest : IRequest<InfiniteData<TransactionDto>>
{
    public string AccountId { get; init; }
    public string Description { get; init; }
    public string TagId { get; init; }
    public TransactionType? Type { get; init; }
    public int Cursor { get; init; }
    public int PageSize { get; init; } = 25;
}

public class GetPaginatedTransactionsQueryHandler : IRequestHandler<GetPaginatedTransactionsRequest, InfiniteData<TransactionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public GetPaginatedTransactionsQueryHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<InfiniteData<TransactionDto>> Handle(GetPaginatedTransactionsRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();

        var query = _dbContext
            .Transactions
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .Where(o => o.Account.OwnerId == userId)
            .OrderByDescending(o => o.ExecutionDate ?? DateTime.MaxValue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.AccountId)) query = query.Where(o => o.Account.Id == request.AccountId);

        if (!string.IsNullOrWhiteSpace(request.Description)) query = query.Where(o => o.Description.ToLower().Contains(request.Description.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.TagId)) query = query.Where(o => o.Tag.Id == request.TagId);

        if (request.Type.HasValue) query = query.Where(o => o.Type == request.Type);

        var totalSize = await query.CountAsync(cancellationToken);
        var nextCursor = request.Cursor + request.PageSize < totalSize ? request.Cursor + request.PageSize : (int?) null;

        var list = await query
            .Skip(request.Cursor)
            .Take(request.PageSize)
            .Select(o => new TransactionDto
            {
                Id = o.Id,
                Description = o.Description,
                TagId = o.Tag != null ? o.Tag.Id : null,
                TagName = o.Tag != null ? o.Tag.Name : null,
                TagColor = o.Tag != null ? o.Tag.Color : null,
                Type = o.Type,
                AccountId = o.Account.Id,
                AccountName = o.Account.Name,
                Amount = o.Amount,
                CreationDate = o.CreationDate,
                ExecutionDate = o.ExecutionDate
            })
            .ToListAsync(cancellationToken);
        return new InfiniteData<TransactionDto>(list, nextCursor);
    }
}