using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.CreateAccount;

public sealed class CreateAccountHandler : IRequestHandler<CreateAccountCommand, ErrorOr<CreateAccountResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;
    private readonly TimeProvider _timeProvider;

    public CreateAccountHandler(IAppDbContext dbContext, IAuthContext authContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _authContext = authContext;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<CreateAccountResponse>> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        var createdAt = _timeProvider.GetUtcNow();

        var accountResult = Account.Create(
            _authContext.CurrentUserId,
            command.Name,
            command.Type,
            command.InitialBalance,
            createdAt
        );

        if (accountResult.IsError)
        {
            return accountResult.Errors;
        }

        var account = accountResult.Value;

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateAccountResponse(
            account.Id,
            account.Name,
            account.Type,
            account.Balance,
            account.CreatedAt,
            account.UpdatedAt
        );
    }
}
