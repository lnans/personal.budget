using Application.Extensions;
using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public sealed class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand, DeleteAccountResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public DeleteAccountHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<DeleteAccountResponse>> Handle(
        DeleteAccountCommand command,
        CancellationToken cancellationToken
    ) =>
        await GetAccountByIdAsync(command.Id, cancellationToken)
            .Then(account => account.Delete(_timeProvider.GetUtcNow()))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                account =>
                    new DeleteAccountResponse(
                        account.Id,
                        account.Name,
                        account.Bank,
                        account.Type,
                        account.InitialBalance,
                        account.Balance,
                        account.CreatedAt,
                        account.UpdatedAt,
                        account.DeletedAt!.Value
                    ).ToErrorOr(),
                error => error
            );

    private async Task<ErrorOr<Account>> GetAccountByIdAsync(Guid accountId, CancellationToken cancellationToken) =>
        await _dbContext
            .Accounts.Include(a => a.Operations)
            .FirstOrErrorAsync(
                account => account.Id == accountId && account.UserId == _authContext.CurrentUserId,
                AccountErrors.AccountNotFound,
                cancellationToken
            );
}
