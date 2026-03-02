using Application.Interfaces;

namespace Application.Features.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountCommand : ICommand<UpdateAccountResponse>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Bank { get; set; }
    public decimal? InitialBalance { get; set; }
}
