using Application.Common.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Accounts;

public record CreateAccountRequest(string Name, string Bank, string Icon, AccountType Type, decimal InitialBalance) : IRequest<CreateAccountResponse>;

public record CreateAccountResponse(string Id, string Name, string Bank, string Icon, AccountType Type, decimal Balance, DateTime CreationDate);

public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage(Errors.AccountNameRequired);
        
        RuleFor(p => p.Bank)
            .NotEmpty()
            .WithMessage(Errors.AccountBankRequired);

        RuleFor(p => p.Type)
            .IsInEnum()
            .WithMessage(Errors.AccountTypeUnknown);
    }
}

public class CreateAccount : IRequestHandler<CreateAccountRequest, CreateAccountResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public CreateAccount(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<CreateAccountResponse> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var existingAccount = await _dbContext
            .Accounts
            .FirstOrDefaultAsync(a => 
                a.Name.ToLower() == request.Name.ToLower() && 
                a.Bank.ToLower() == request.Bank.ToLower() &&
                a.OwnerId == userId, cancellationToken);

        if (existingAccount is not null) throw new AlreadyExistException(Errors.AccountAlreadyExist);

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
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

        return new CreateAccountResponse(account.Id, account.Name, account.Bank, account.Icon, account.Type, account.Balance, account.CreationDate);
    }
}