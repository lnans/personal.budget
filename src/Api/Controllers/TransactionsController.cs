using Application.Features.Transactions.Commands.CreateTransactions;
using Application.Features.Transactions.Commands.DeleteTransaction;
using Application.Features.Transactions.Commands.PatchTransaction;
using Application.Features.Transactions.Queries.GetPaginatedTransactions;

namespace Api.Controllers;

[Authorize]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    ///     Get all transactions
    /// </summary>
    /// <param name="request">Filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] GetPaginatedTransactionsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Create a new account transaction
    /// </summary>
    /// <param name="request">transaction form</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>transaction created</returns>
    [HttpPost]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Update Description, Tag and amount of an transaction
    /// </summary>
    /// <param name="id">transaction id</param>
    /// <param name="request">transaction form</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] PatchTransactionRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Delete an account transaction
    /// </summary>
    /// <param name="id">transaction Id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var request = new DeleteTransactionRequest {Id = id};
        await _mediator.Send(request, cancellationToken);
        return Ok();
    }
}