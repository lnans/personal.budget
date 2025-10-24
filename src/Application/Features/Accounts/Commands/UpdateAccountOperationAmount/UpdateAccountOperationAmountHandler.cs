using Application.Interfaces;
using Domain.AccountOperations;
using Domain.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.UpdateAccountOperationAmount;

public sealed class UpdateAccountOperationAmountHandler
    : IRequestHandler<UpdateAccountOperationAmountCommand, ErrorOr<UpdateAccountOperationAmountResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public UpdateAccountOperationAmountHandler(
        IAppDbContext dbContext,
        IAuthContext authContext,
        TimeProvider timeProvider
    )
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<UpdateAccountOperationAmountResponse>> Handle(
        UpdateAccountOperationAmountCommand command,
        CancellationToken cancellationToken
    )
    {
        // First, get the target operation to know its CreatedAt timestamp
        var targetOperation = await _dbContext
            .AccountOperations.AsNoTracking()
            .Where(o =>
                o.Id == command.OperationId
                && o.AccountId == command.AccountId
                && o.Account.UserId == _authContext.CurrentUserId
            )
            .Select(o => new { o.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (targetOperation is null)
        {
            return AccountOperationErrors.AccountOperationNotFound;
        }

        // Load the account with only the target operation and subsequent operations
        var account = await _dbContext
            .Accounts.Include(a =>
                a.Operations.Where(o => o.Id == command.OperationId || o.CreatedAt > targetOperation.CreatedAt)
            )
            .FirstOrDefaultAsync(
                a => a.Id == command.AccountId && a.UserId == _authContext.CurrentUserId,
                cancellationToken
            );

        if (account is null)
        {
            return AccountErrors.AccountNotFound;
        }

        var updatedAt = _timeProvider.GetUtcNow();
        var updateResult = account.UpdateOperationAmount(command.OperationId, command.Amount, updatedAt);

        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var operation = account.Operations.First(o => o.Id == command.OperationId);

        return new UpdateAccountOperationAmountResponse(
            operation.Id,
            operation.AccountId,
            operation.Description,
            operation.Amount,
            operation.PreviousBalance,
            operation.NextBalance,
            operation.CreatedAt,
            operation.UpdatedAt
        );
    }
}
