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
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(
            a => a.Id == command.Id && a.UserId == _authContext.CurrentUserId,
            cancellationToken
        );

        if (account is null)
        {
            return AccountErrors.AccountNotFound;
        }

        var updatedAt = _timeProvider.GetUtcNow();
        var renameResult = account.Rename(command.Name, updatedAt);

        if (renameResult.IsError)
        {
            return renameResult.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PatchAccountResponse(
            account.Id,
            account.Name,
            account.Balance,
            account.CreatedAt,
            account.UpdatedAt
        );
    }
}
