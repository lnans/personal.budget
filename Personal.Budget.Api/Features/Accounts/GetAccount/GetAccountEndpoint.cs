namespace Personal.Budget.Api.Features.Accounts.GetAccount;

public class GetAccountEndpoint : Endpoint<GetAccountRequest, GetAccountResponse>
{
    public override void Configure()
    {
        Get("accounts/{id:guid}");
    }

    public override async Task HandleAsync(GetAccountRequest req, CancellationToken ct)
    {
        await SendOkAsync(new GetAccountResponse {Name = "test"}, ct);
    }
}