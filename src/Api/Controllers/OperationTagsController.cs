using Application.Commands.OperationTag;
using Application.Queries.OperationTag;
using Domain.Common;

namespace Api.Controllers;

[Route("[controller]")]
public class OperationTagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperationTagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Get all operation tag available
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetAllOperationTagResponse>), (int) HttpStatusCode.OK)]
    public async Task<IActionResult> GetAllOperationTags(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllOperationTagQuery(), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Create a new operation TAG
    /// </summary>
    /// <param name="request">Operation TAG to create</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Operation TAG created</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOperationTagResponse), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateOperationTag([FromBody] CreateOperationTagCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Update an operation TAG
    /// </summary>
    /// <param name="id">Operation tag id</param>
    /// <param name="request">Operation tag properties</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateOperationTagResponse), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateOperationTag(string id, [FromBody] UpdateOperationTagRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOperationTagCommand(id, request.Name, request.Color);
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Delete en operation TAG
    /// </summary>
    /// <param name="id">Operation tag ID</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType((int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeleteOperationTag(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteOperationTagCommand(id), cancellationToken);
        return Ok();
    }
}