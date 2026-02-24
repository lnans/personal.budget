using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.AddAccountOperation;

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
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(
            a => a.Id == command.AccountId && a.UserId == _authContext.CurrentUserId,
            cancellationToken
        );

        if (account is null)
        {
            return AccountErrors.AccountNotFound;
        }

        var createdAt = _timeProvider.GetUtcNow();
        var addOperationResult = account.AddOperation(command.Description, command.Amount, createdAt);

        if (addOperationResult.IsError)
        {
            return addOperationResult.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var operation = account.Operations.Last();

        return new AddAccountOperationResponse(
            operation.Id,
            operation.AccountId,
            account.Name,
            operation.Description,
            operation.Amount,
            operation.PreviousBalance,
            operation.NextBalance,
            operation.CreatedAt,
            operation.UpdatedAt
        );
    }
}
