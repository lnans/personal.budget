using Application.Extensions;
using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;

namespace Application.Features.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountHandler : ICommandHandler<UpdateAccountCommand, UpdateAccountResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public UpdateAccountHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<UpdateAccountResponse>> Handle(
        UpdateAccountCommand command,
        CancellationToken cancellationToken
    ) =>
        await GetAccountByIdAsync(command.Id, command.InitialBalance.HasValue, cancellationToken)
            .Then(account =>
                account.Patch(command.Name, command.Bank, command.InitialBalance, _timeProvider.GetUtcNow())
            )
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                account =>
                    new UpdateAccountResponse(
                        account.Id,
                        account.Name,
                        account.Bank,
                        account.Type,
                        account.InitialBalance,
                        account.Balance,
                        account.CreatedAt,
                        account.UpdatedAt
                    ).ToErrorOr(),
                error => error
            );

    private async Task<ErrorOr<Account>> GetAccountByIdAsync(
        Guid accountId,
        bool includeOperations,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .Accounts.IncludeIf(includeOperations, a => a.Operations.Where(o => o.DeletedAt == null))
            .FirstOrErrorAsync(
                account => account.Id == accountId && account.UserId == _authContext.CurrentUserId,
                AccountErrors.AccountNotFound,
                cancellationToken
            );
}
