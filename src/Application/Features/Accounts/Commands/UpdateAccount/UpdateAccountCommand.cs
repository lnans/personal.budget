using Application.Interfaces;

namespace Application.Features.Accounts.Commands.UpdateAccount;

public sealed record UpdateAccountCommand(Guid Id, string Name, string Bank, decimal? InitialBalance = null)
    : ICommand<UpdateAccountResponse>;
