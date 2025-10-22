using Application.Interfaces;
using Domain.AccountOperations;
using Domain.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.RenameAccountOperation;

public sealed class RenameAccountOperationHandler
    : IRequestHandler<RenameAccountOperationCommand, ErrorOr<RenameAccountOperationResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public RenameAccountOperationHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<RenameAccountOperationResponse>> Handle(
        RenameAccountOperationCommand command,
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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RenameAccountOperationResponse(
            operation.Id,
            operation.AccountId,
            operation.Description,
            operation.Amount,
            operation.PreviousBalance,
            operation.NextBalance,
            operation.CreatedAt,
            operation.UpdatedAt
        );
    }
}
