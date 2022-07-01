using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.Commands.CreateOperations;

public record CreateOperationsRequest : IRequest<Unit>
{
    public string AccountId { get; init; }
    public CreateOperationDto[] Operations { get; init; }
}

public class CreateOperationsCommandHandler : IRequestHandler<CreateOperationsRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public CreateOperationsCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(CreateOperationsRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        var operations = new List<Operation>();
        foreach (var requestOperation in request.Operations)
        {
            var operation = new Operation
            {
                Id = Guid.NewGuid().ToString(),
                Description = requestOperation.Description,
                Account = account,
                Amount = requestOperation.Amount,
                Type = requestOperation.OperationType,
                CreationDate = requestOperation.CreationDate,
                ExecutionDate = requestOperation.ExecutionDate,
                CreatedById = userId
            };

            if (!string.IsNullOrWhiteSpace(requestOperation.OperationTagId))
            {
                var tag = await _dbContext
                    .OperationTags
                    .FirstOrDefaultAsync(o => o.Id == requestOperation.OperationTagId && o.OwnerId == userId, cancellationToken);

                if (tag is null) throw new NotFoundException(Errors.OperationTagNotFound);

                operation.Tag = tag;
            }

            // Update account balance
            if (requestOperation.ExecutionDate.HasValue) account.Balance += requestOperation.Amount;

            operations.Add(operation);
        }

        await _dbContext.Operations.AddRangeAsync(operations, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}