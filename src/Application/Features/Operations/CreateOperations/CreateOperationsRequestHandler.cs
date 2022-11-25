using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.CreateOperations;

internal sealed class CreateOperationsRequestHandler : IRequestHandler<CreateOperationsRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public CreateOperationsRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(CreateOperationsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(ErrorsAccounts.NotFound);

        var operations = new List<Operation>();
        foreach (var operationData in request.Operations)
        {
            var operation = new Operation
            {
                Description = operationData.Description!,
                Account = account,
                Amount = operationData.Amount,
                Type = operationData.Type,
                CreationDate = operationData.CreationDate,
                ExecutionDate = operationData.ExecutionDate,
                CreatedById = userId
            };

            if (operationData.TagIds != null)
            {
                var tags = new List<Tag>();
                foreach (var tagId in operationData.TagIds)
                {
                    var tag = await _dbContext
                        .Tags
                        .FirstOrDefaultAsync(o => o.Id == tagId && o.OwnerId == userId, cancellationToken);

                    if (tag is null) throw new NotFoundException(ErrorsTags.NotFound);
                }

                operation.Tags = tags;
            }

            // Update account balance
            account.Balance += operationData.Amount;

            operations.Add(operation);
        }

        await _dbContext.Operations.AddRangeAsync(operations, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}