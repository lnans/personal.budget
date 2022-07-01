using Application.Common.Interfaces;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.Queries.GetPaginatedOperations;

public record GetPaginatedOperationsRequest : IRequest<InfiniteData<OperationDto>>
{
    public string AccountId { get; init; }
    public string Description { get; init; }
    public string[] TagIds { get; init; }
    public OperationType? Type { get; init; }
    public int Cursor { get; init; }
    public int PageSize { get; init; } = 25;
}

public class GetPaginatedOperationsQueryHandler : IRequestHandler<GetPaginatedOperationsRequest, InfiniteData<OperationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public GetPaginatedOperationsQueryHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<InfiniteData<OperationDto>> Handle(GetPaginatedOperationsRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();

        var query = _dbContext
            .Operations
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .Where(o => o.Account.OwnerId == userId)
            .OrderByDescending(o => o.ExecutionDate ?? DateTime.MaxValue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.AccountId)) query = query.Where(o => o.Account.Id == request.AccountId);

        if (!string.IsNullOrWhiteSpace(request.Description)) query = query.Where(o => o.Description.ToLower().Contains(request.Description.ToLower()));

        if (request.TagIds is not null && request.TagIds.Length > 0) query = query.Where(o => request.TagIds.Contains(o.Tag.Id));

        if (request.Type.HasValue) query = query.Where(o => o.Type == request.Type);

        var totalSize = await query.CountAsync(cancellationToken);
        var nextCursor = request.Cursor + request.PageSize < totalSize ? request.Cursor + request.PageSize : (int?) null;

        var list = await query
            .Skip(request.Cursor)
            .Take(request.PageSize)
            .Select(o => new OperationDto
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
        return new InfiniteData<OperationDto>(list, nextCursor);
    }
}