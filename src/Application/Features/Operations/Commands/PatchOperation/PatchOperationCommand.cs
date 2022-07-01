using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.Commands.PatchOperation;

public record PatchOperationRequest : IRequest<Unit>
{
    public string Id { get; set; }
    public string Description { get; init; }
    public string TagId { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}

public class PatchOperationCommandHandler : IRequestHandler<PatchOperationRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public PatchOperationCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(PatchOperationRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var operation = await _dbContext
            .Operations
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.Account.OwnerId == userId, cancellationToken);

        if (operation is null) throw new NotFoundException(Errors.OperationNotFound);

        if (!string.IsNullOrWhiteSpace(request.TagId))
        {
            var tag = await _dbContext
                .OperationTags
                .FirstOrDefaultAsync(o => o.Id == request.TagId && o.OwnerId == userId, cancellationToken);

            if (tag is null) throw new NotFoundException(Errors.OperationTagNotFound);

            operation.Tag = tag;
        }
        else if (operation.Tag is not null)
        {
            operation.Tag = null;
        }

        // Update operation Amount
        if (request.ExecutionDate.HasValue && operation.ExecutionDate.HasValue)
            operation.Account.Balance += request.Amount - operation.Amount;
        // Execute operation
        else if (request.ExecutionDate.HasValue && !operation.ExecutionDate.HasValue)
            operation.Account.Balance += request.Amount;
        // Cancel operation
        else if (!request.ExecutionDate.HasValue && operation.ExecutionDate.HasValue) operation.Account.Balance -= operation.Amount;

        operation.Description = request.Description;
        operation.Amount = request.Amount;
        operation.CreationDate = request.CreationDate;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}