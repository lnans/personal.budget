using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Accounts;

public record ArchivedAccountRequest(bool Archived);

public record ArchivedAccountRequestWithId(string Id, ArchivedAccountRequest Request) : IRequest<Unit>;

public class ArchivedAccount : IRequestHandler<ArchivedAccountRequestWithId, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public ArchivedAccount(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(ArchivedAccountRequestWithId requestWithId, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == requestWithId.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        account.Archived = requestWithId.Request.Archived;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}