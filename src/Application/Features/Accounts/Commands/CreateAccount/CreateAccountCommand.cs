using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.CreateAccount;

public record CreateAccountRequest : IRequest<Guid>
{
    public string Name { get; init; }
    public string Bank { get; init; }
    public string Icon { get; init; }
    public AccountType Type { get; init; }
    public decimal InitialBalance { get; init; }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountRequest, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public CreateAccountCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<Guid> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var existingAccount = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a =>
                a.Name.ToLower() == request.Name.ToLower() &&
                a.Bank.ToLower() == request.Bank.ToLower() &&
                a.OwnerId == userId, cancellationToken);

        if (existingAccount is not null) throw new AlreadyExistException(Errors.AccountAlreadyExist);

        var guid = Guid.NewGuid();
        var account = new Account
        {
            Id = guid.ToString(),
            OwnerId = userId,
            Name = request.Name,
            Bank = request.Bank,
            InitialBalance = request.InitialBalance,
            Balance = request.InitialBalance,
            Icon = request.Icon,
            Type = request.Type,
            CreationDate = DateTime.UtcNow,
            Archived = false
        };
        await _dbContext.Accounts.AddAsync(account, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return guid;
    }
}