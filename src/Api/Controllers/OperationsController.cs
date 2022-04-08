using Application.Commands.Operations;

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
    ///     Create a new account operation
    /// </summary>
    /// <param name="request">operation form</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>Operation created</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOperationResponse), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOperationRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
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