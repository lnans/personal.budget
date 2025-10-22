using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommand : IRequest<ErrorOr<CreateAccountResponse>>
{
    public required string Name { get; set; }
    public required decimal InitialBalance { get; set; }
}
