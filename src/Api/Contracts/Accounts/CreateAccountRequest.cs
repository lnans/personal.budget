using Domain.Accounts;

namespace Api.Contracts.Accounts;

public sealed record CreateAccountRequest(string Name, string Bank, AccountType Type, decimal InitialBalance);
