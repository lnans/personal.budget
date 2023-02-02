using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.ArchiveAccount;

internal sealed class ArchiveAccountRequestHandler : IRequestHandler<ArchiveAccountRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public ArchiveAccountRequestHandler(IApplicationDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(ArchiveAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _authContext.GetAuthenticatedUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(ErrorsAccounts.NotFound);

        account.Archived = request.Archived;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}