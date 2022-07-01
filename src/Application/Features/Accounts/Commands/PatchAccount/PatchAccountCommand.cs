using Application.Common.Interfaces;
using Domain;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.PatchAccount;

public record PatchAccountRequest : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; init; }
    public string Bank { get; init; }
    public string Icon { get; init; }
}

public class PatchAccountCommandHandler : IRequestHandler<PatchAccountRequest, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public PatchAccountCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Unit> Handle(PatchAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        account.Name = request.Name;
        account.Bank = request.Bank;
        account.Icon = request.Icon;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}