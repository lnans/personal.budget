using Application.Interfaces;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public sealed class DeleteAccountCommand : ICommand<DeleteAccountResponse>
{
    public Guid Id { get; set; }
}
