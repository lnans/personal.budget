using Application.Interfaces;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public sealed record DeleteAccountCommand(Guid Id) : ICommand<DeleteAccountResponse>;
