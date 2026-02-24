using Application.Interfaces;
using Domain.Accounts;

namespace Application.Features.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommand : ICommand<CreateAccountResponse>
{
    public required string Name { get; set; }
    public required string Bank { get; set; }
    public required AccountType Type { get; set; }
    public required decimal InitialBalance { get; set; }
}
