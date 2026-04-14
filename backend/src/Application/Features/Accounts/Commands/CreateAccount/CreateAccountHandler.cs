using Application.Interfaces;
using Domain.Accounts;
using ErrorOr;

namespace Application.Features.Accounts.Commands.CreateAccount;

public sealed class CreateAccountHandler : ICommandHandler<CreateAccountCommand, CreateAccountResponse>
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
    ) =>
        await Account
            .Create(
                _authContext.CurrentUserId,
                command.Name,
                command.Bank,
                command.Type,
                command.InitialBalance,
                _timeProvider.GetUtcNow()
            )
            .ThenDo(account => _dbContext.Accounts.Add(account))
            .ThenDoAsync(_ => _dbContext.SaveChangesAsync(cancellationToken))
            .MatchFirst(
                account =>
                    new CreateAccountResponse(
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
}
