using Application.Commands.Operations;
using Application.Queries.Operations;

namespace Api.Controllers;

[Authorize]
[Route("[controller]")]
public class OperationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Get all operations
    /// </summary>
    /// <param name="request">Filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetAllOperationsPaginatedResponse), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllOperationsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Create a new account operation
    /// </summary>
    /// <param name="request">operation form</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Operation created</returns>
    [HttpPost]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOperationsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Update Description, Tag and amount of an operation
    /// </summary>
    /// <param name="id">Operation id</param>
    /// <param name="request">operation form</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(UpdateOperationResponse), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateOperationRequest request, CancellationToken cancellationToken)
    {
        var requestWithId = new UpdateOperationRequestWithId(id, request);
        var response = await _mediator.Send(requestWithId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Set execution date on an operation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("{id}/execute")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    public async Task<IActionResult> Execute(string id, [FromBody] ExecuteOperationRequest request, CancellationToken cancellationToken)
    {
        var requestWithId = new ExecuteOperationRequestWithId(id, request);
        await _mediator.Send(requestWithId, cancellationToken);
        return Ok();
    }

    /// <summary>
    ///     Delete an account operation
    /// </summary>
    /// <param name="id">Operation Id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var request = new DeleteOperationRequest(id);
        await _mediator.Send(request, cancellationToken);
        return Ok();
    }
}