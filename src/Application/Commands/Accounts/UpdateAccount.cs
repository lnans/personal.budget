using Application.Common.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Accounts;

public record UpdateAccountRequest(string Name, string Icon);

public record UpdateAccountRequestWithId(string Id, UpdateAccountRequest Request) : IRequest<UpdateAccountResponse>;

public record UpdateAccountResponse(string Id, string Name, string Icon, AccountType Type, decimal Balance, DateTime CreationDate);

public class UpdateAccountValidator : AbstractValidator<UpdateAccountRequestWithId>
{
    public UpdateAccountValidator()
    {
        RuleFor(p => p.Request.Name)
            .NotEmpty()
            .WithMessage(Errors.AccountNameRequired);
    }
}

public class UpdateAccount : IRequestHandler<UpdateAccountRequestWithId, UpdateAccountResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public UpdateAccount(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }
    
    public async Task<UpdateAccountResponse> Handle(UpdateAccountRequestWithId requestWithId, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var account = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => a.Id == requestWithId.Id && a.OwnerId == userId, cancellationToken);

        if (account is null) throw new NotFoundException(Errors.AccountNotFound);

        account.Name = requestWithId.Request.Name;
        account.Icon = requestWithId.Request.Icon;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateAccountResponse(account.Id, account.Name, account.Icon, account.Type, account.Balance, account.CreationDate);
    }
}