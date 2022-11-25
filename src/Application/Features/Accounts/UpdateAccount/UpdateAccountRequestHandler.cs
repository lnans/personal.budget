using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.UpdateAccount;

internal sealed class UpdateAccountRequestHandler : IRequestHandler<UpdateAccountRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public UpdateAccountRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
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