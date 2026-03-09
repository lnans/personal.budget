using Application.Extensions;
using Application.Interfaces;
using Domain.AccountOperations;
using Domain.Accounts;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AccountOperations.Commands.DeleteAccountOperation;

public sealed class DeleteAccountOperationHandler
    : ICommandHandler<DeleteAccountOperationCommand, DeleteAccountOperationResponse>
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
        var deletedAt = _timeProvider.GetUtcNow();

        return await GetAccountOperationAsync(command.AccountId, command.OperationId, cancellationToken)
            .ThenAsync(operationToDelete => GetAccountWithOperationsAsync(operationToDelete, cancellationToken))
            .Then(account => account.DeleteOperation(command.OperationId, deletedAt))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                account => account.Operations.First(o => o.Id == command.OperationId).ToErrorOr(),
                error => error
            )
            .MatchFirst(
                operation =>
                    new DeleteAccountOperationResponse(
                        operation.Id,
                        operation.AccountId,
                        operation.Account.Name,
                        operation.Description,
                        operation.Amount,
                        operation.PreviousBalance,
                        operation.NextBalance,
                        operation.OperationDate,
                        operation.CreatedAt,
                        operation.UpdatedAt,
                        operation.DeletedAt!.Value
                    ).ToErrorOr(),
                error => error
            );
    }

    private async Task<ErrorOr<AccountOperation>> GetAccountOperationAsync(
        Guid accountId,
        Guid operationId,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .AccountOperations.AsNoTracking()
            .FirstOrErrorAsync(
                accountOperation =>
                    accountOperation.Id == operationId
                    && accountOperation.AccountId == accountId
                    && accountOperation.Account.UserId == _authContext.CurrentUserId,
                AccountOperationErrors.AccountOperationNotFound,
                cancellationToken
            );

    private async Task<ErrorOr<Account>> GetAccountWithOperationsAsync(
        AccountOperation operationToDelete,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .Accounts.Include(account =>
                account.Operations.Where(accountOperation =>
                    accountOperation.Id == operationToDelete.Id
                    || accountOperation.CreatedAt > operationToDelete.CreatedAt
                )
            )
            .FirstOrErrorAsync(
                account => account.Id == operationToDelete.AccountId && account.UserId == _authContext.CurrentUserId,
                AccountErrors.AccountNotFound,
                cancellationToken
            );
}
