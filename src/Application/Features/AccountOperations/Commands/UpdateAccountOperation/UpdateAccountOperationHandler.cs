using Application.Extensions;
using Application.Interfaces;
using Domain.AccountOperations;
using Domain.Accounts;
using Domain.Tags;
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
        var updatedAt = _timeProvider.GetUtcNow();
        var tags = await GetTagsAsync(command.TagIds ?? [], cancellationToken);

        return await tags.ThenAsync(_ =>
                GetAccountOperationAsync(command.AccountId, command.OperationId, cancellationToken)
            )
            .Then(operation => operation.Rename(command.Description, updatedAt))
            .Then(operation => UpdateOperationDate(operation, command, updatedAt))
            .ThenAsync(operation => UpdateOperationAmountAsync(operation, command, updatedAt, cancellationToken))
            .ThenDo(operation => operation.UpdateRecurring(command.IsRecurring, updatedAt))
            .Then(operation => operation.UpdateTags(tags.Value, updatedAt))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                operation =>
                    new UpdateAccountOperationResponse(
                        operation.Id,
                        operation.AccountId,
                        operation.Account.Name,
                        operation.Description,
                        operation.Amount,
                        operation.PreviousBalance,
                        operation.NextBalance,
                        operation.IsRecurring,
                        operation
                            .Tags.Select(tag => new UpdateAccountOperationTagResponse(tag.Id, tag.Name, tag.Color))
                            .ToList(),
                        operation.OperationDate,
                        operation.CreatedAt,
                        operation.UpdatedAt
                    ).ToErrorOr(),
                errors => errors
            );
    }

    private async Task<ErrorOr<AccountOperation>> GetAccountOperationAsync(
        Guid accountId,
        Guid operationId,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .AccountOperations.Include(o => o.Account)
            .Include(o => o.Tags)
            .FirstOrErrorAsync(
                o => o.Id == operationId && o.AccountId == accountId && o.Account.UserId == _authContext.CurrentUserId,
                AccountOperationErrors.AccountOperationNotFound,
                cancellationToken
            );

    private async Task<ErrorOr<List<Tag>>> GetTagsAsync(
        IReadOnlyList<Guid> tagIds,
        CancellationToken cancellationToken
    ) =>
        await _dbContext.Tags.FindAllWithExpectedCountOrErrorAsync(
            tagIds.Count,
            tag => tag.UserId == _authContext.CurrentUserId && tagIds.Contains(tag.Id),
            TagErrors.TagNotFound,
            cancellationToken
        );

    private static ErrorOr<AccountOperation> UpdateOperationDate(
        AccountOperation operation,
        UpdateAccountOperationCommand command,
        DateTimeOffset updatedAt
    ) => command.OperationDate.HasValue ? operation.UpdateDate(command.OperationDate.Value, updatedAt) : operation;

    private async Task<ErrorOr<AccountOperation>> UpdateOperationAmountAsync(
        AccountOperation operation,
        UpdateAccountOperationCommand command,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken
    ) =>
        operation.Amount == command.Amount
            ? operation
            : await _dbContext
                .Accounts.Include(a =>
                    a.Operations.Where(o => o.Id == command.OperationId || o.CreatedAt > operation.CreatedAt)
                )
                .FirstOrErrorAsync(
                    a => a.Id == command.AccountId && a.UserId == _authContext.CurrentUserId,
                    AccountErrors.AccountNotFound,
                    cancellationToken
                )
                .Then(account => account.UpdateOperationAmount(command.OperationId, command.Amount, updatedAt))
                .MatchFirst(_ => operation.ToErrorOr(), error => error);
}
