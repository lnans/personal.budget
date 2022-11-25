using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.CreateAccount;

public class CreateAccountRequestHandler : IRequestHandler<CreateAccountRequest, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public CreateAccountRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Guid> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var existingAccount = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a =>
                a.Name.ToLower() == request.Name!.ToLower() &&
                a.Bank.ToLower() == request.Bank!.ToLower() &&
                a.OwnerId == userId, cancellationToken);

        if (existingAccount is not null) throw new ConflictException(ErrorsAccounts.AlreadyExist);

        var account = new Account
        {
            OwnerId = userId,
            Name = request.Name!,
            Bank = request.Bank!,
            InitialBalance = request.InitialBalance,
            Balance = request.InitialBalance,
            Type = request.Type,
            CreationDate = DateTime.UtcNow,
            Archived = false
        };
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}