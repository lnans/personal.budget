using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.UpdateAccount;

internal sealed class UpdateAccountRequestHandler : IRequestHandler<UpdateAccountRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public UpdateAccountRequestHandler(IApplicationDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _authContext.GetAuthenticatedUserId();

        var existingAccount = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a =>
                a.Name.ToLower() == request.Name!.ToLower() &&
                a.Bank.ToLower() == request.Bank!.ToLower() &&
                a.OwnerId == userId, cancellationToken);

        if (existingAccount is not null) throw new ConflictException(ErrorsAccounts.AlreadyExist);

        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(ErrorsAccounts.NotFound);

        account.Name = request.Name!;
        account.Bank = request.Bank!;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}