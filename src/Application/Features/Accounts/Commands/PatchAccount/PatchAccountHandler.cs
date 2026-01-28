using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.PatchAccount;

public sealed class PatchAccountHandler : IRequestHandler<PatchAccountCommand, ErrorOr<PatchAccountResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public PatchAccountHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<PatchAccountResponse>> Handle(
        PatchAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        var accountQuery = _dbContext.Accounts.Where(a => a.Id == command.Id && a.UserId == _authContext.CurrentUserId);

        // If InitialBalance is being updated, we need to load operations to recalculate balances
        if (command.InitialBalance.HasValue)
        {
            accountQuery = accountQuery.Include(a => a.Operations.Where(o => o.DeletedAt == null));
        }

        var account = await accountQuery.FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            return AccountErrors.AccountNotFound;
        }

        var updatedAt = _timeProvider.GetUtcNow();
        var patchResult = account.Patch(command.Name, command.Bank, command.InitialBalance, updatedAt);

        if (patchResult.IsError)
        {
            return patchResult.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PatchAccountResponse(
            account.Id,
            account.Name,
            account.Bank,
            account.Type,
            account.InitialBalance,
            account.Balance,
            account.CreatedAt,
            account.UpdatedAt
        );
    }
}
