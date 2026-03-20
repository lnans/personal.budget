using Application.Extensions;
using Application.Interfaces;
using Domain.Accounts;
using Domain.Tags;
using ErrorOr;

namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

public sealed class AddAccountOperationHandler
    : ICommandHandler<AddAccountOperationCommand, AddAccountOperationResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public AddAccountOperationHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<AddAccountOperationResponse>> Handle(
        AddAccountOperationCommand command,
        CancellationToken cancellationToken
    )
    {
        var createdAt = _timeProvider.GetUtcNow();
        var operationDate = command.OperationDate ?? createdAt;
        var tags = await GetTagsAsync(command.TagIds ?? [], cancellationToken);

        return await tags.ThenAsync(_ => GetAccountAsync(command.AccountId, cancellationToken))
            .Then(account =>
                account.AddOperation(command.Description, command.Amount, command.IsRecurring, operationDate, createdAt)
            )
            .MatchFirst(account => account.Operations[^1].ToErrorOr(), error => error)
            .Then(operation => operation.UpdateTags(tags.Value, createdAt))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                operation =>
                    new AddAccountOperationResponse(
                        operation.Id,
                        operation.AccountId,
                        operation.Account.Name,
                        operation.Description,
                        operation.Amount,
                        operation.PreviousBalance,
                        operation.NextBalance,
                        operation.IsRecurring,
                        operation
                            .Tags.Select(tag => new AddAccountOperationTagResponse(tag.Id, tag.Name, tag.Color))
                            .ToList(),
                        operation.OperationDate,
                        operation.CreatedAt,
                        operation.UpdatedAt
                    ).ToErrorOr(),
                error => error
            );
    }

    private async Task<ErrorOr<Account>> GetAccountAsync(Guid accountId, CancellationToken cancellationToken) =>
        await _dbContext.Accounts.FirstOrErrorAsync(
            account => account.Id == accountId && account.UserId == _authContext.CurrentUserId,
            AccountErrors.AccountNotFound,
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
}
