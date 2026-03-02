using Application.Interfaces;
using Domain.Accounts;

namespace Application.Features.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(string Name, string Bank, AccountType Type, decimal InitialBalance)
    : ICommand<CreateAccountResponse>;
