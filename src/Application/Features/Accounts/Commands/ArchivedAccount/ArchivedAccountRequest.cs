using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.ArchivedAccount;

public class ArchivedAccountRequest : IRequest<Unit>
{
    public string Id { get; set; }
    public bool Archived { get; set; }
}

public class ArchivedAccountCommandHandler : IRequestHandler<ArchivedAccountRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public ArchivedAccountCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(ArchivedAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        account.Archived = request.Archived;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}