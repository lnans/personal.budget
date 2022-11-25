using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.ArchiveAccount;

internal sealed class ArchiveAccountRequestHandler : IRequestHandler<ArchiveAccountRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public ArchiveAccountRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(ArchiveAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(ErrorsAccounts.NotFound);

        account.Archived = request.Archived;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}