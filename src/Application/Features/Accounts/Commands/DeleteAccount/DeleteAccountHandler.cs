using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public sealed class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, ErrorOr<DeleteAccountResponse>>
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
    )
    {
        // Include operations so they are loaded and can be soft-deleted by the aggregate
        var account = await _dbContext
            .Accounts.Include(a => a.Operations)
            .FirstOrDefaultAsync(a => a.Id == command.Id && a.UserId == _authContext.CurrentUserId, cancellationToken);

        if (account is null)
        {
            return AccountErrors.AccountNotFound;
        }

        var deletedAt = _timeProvider.GetUtcNow();
        var deleteResult = account.Delete(deletedAt);

        if (deleteResult.IsError)
        {
            return deleteResult.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteAccountResponse(
            account.Id,
            account.Name,
            account.Type,
            account.Balance,
            account.CreatedAt,
            account.UpdatedAt,
            account.DeletedAt!.Value
        );
    }
}
