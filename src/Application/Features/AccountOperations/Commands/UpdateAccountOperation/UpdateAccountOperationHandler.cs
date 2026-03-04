using Application.Interfaces;
using Domain.AccountOperations;
using Domain.Accounts;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AccountOperations.Commands.UpdateAccountOperation;

public sealed class UpdateAccountOperationHandler
    : ICommandHandler<UpdateAccountOperationCommand, UpdateAccountOperationResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public UpdateAccountOperationHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<UpdateAccountOperationResponse>> Handle(
        UpdateAccountOperationCommand command,
        CancellationToken cancellationToken
    )
    {
        var operation = await _dbContext
            .AccountOperations.Include(o => o.Account)
            .FirstOrDefaultAsync(
                o => o.Id == command.OperationId && o.AccountId == command.AccountId,
                cancellationToken
            );

        if (operation is null)
        {
            return AccountOperationErrors.AccountOperationNotFound;
        }

        if (operation.Account.UserId != _authContext.CurrentUserId)
        {
            return AccountErrors.AccountNotFound;
        }

        var updatedAt = _timeProvider.GetUtcNow();
        var renameResult = operation.Rename(command.Description, updatedAt);

        if (renameResult.IsError)
        {
            return renameResult.Errors;
        }

        if (operation.Amount != command.Amount)
        {
            var account = await _dbContext
                .Accounts.Include(a =>
                    a.Operations.Where(o => o.Id == command.OperationId || o.CreatedAt > operation.CreatedAt)
                )
                .FirstOrDefaultAsync(
                    a => a.Id == command.AccountId && a.UserId == _authContext.CurrentUserId,
                    cancellationToken
                );

            if (account is null)
            {
                return AccountErrors.AccountNotFound;
            }

            var updateResult = account.UpdateOperationAmount(command.OperationId, command.Amount, updatedAt);

            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateAccountOperationResponse(
            operation.Id,
            operation.AccountId,
            operation.Account.Name,
            operation.Description,
            operation.Amount,
            operation.PreviousBalance,
            operation.NextBalance,
            operation.CreatedAt,
            operation.UpdatedAt
        );
    }
}
