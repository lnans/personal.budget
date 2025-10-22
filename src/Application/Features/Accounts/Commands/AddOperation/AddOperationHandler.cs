using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.AddOperation;

public sealed class AddOperationHandler : IRequestHandler<AddOperationCommand, ErrorOr<AddOperationResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public AddOperationHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<AddOperationResponse>> Handle(
        AddOperationCommand command,
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

        return new AddOperationResponse(
            account.Id,
            account.Name,
            account.Balance,
            account.CreatedAt,
            account.UpdatedAt
        );
    }
}
