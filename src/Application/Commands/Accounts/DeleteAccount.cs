using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Accounts;

public record DeleteAccountRequest(string Id) : IRequest<Unit>;

public class DeleteAccount : IRequestHandler<DeleteAccountRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public DeleteAccount(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Unit> Handle(DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        _dbContext.Accounts.Remove(account);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}