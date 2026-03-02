namespace Api.Contracts.Accounts;

public sealed record UpdateAccountRequest(string Name, string Bank, decimal? InitialBalance = null);
