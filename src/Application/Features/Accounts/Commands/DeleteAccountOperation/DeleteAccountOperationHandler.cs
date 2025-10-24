using Application.Interfaces;
using Domain.AccountOperations;
using Domain.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.DeleteAccountOperation;

public sealed class DeleteAccountOperationHandler
    : IRequestHandler<DeleteAccountOperationCommand, ErrorOr<DeleteAccountOperationResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public DeleteAccountOperationHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<DeleteAccountOperationResponse>> Handle(
        DeleteAccountOperationCommand command,
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

        // Load the account with the target operation and all subsequent operations
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

        var deletedAt = _timeProvider.GetUtcNow();
        var deleteResult = account.DeleteOperation(command.OperationId, deletedAt);

        if (deleteResult.IsError)
        {
            return deleteResult.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var operation = account.Operations.First(o => o.Id == command.OperationId);

        return new DeleteAccountOperationResponse(
            operation.Id,
            operation.AccountId,
            operation.Description,
            operation.Amount,
            operation.PreviousBalance,
            operation.NextBalance,
            operation.CreatedAt,
            operation.UpdatedAt,
            operation.DeletedAt!.Value
        );
    }
}
