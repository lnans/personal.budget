using Application.Features.Authentication.Commands.SignIn;
using Application.Features.Authentication.Queries.GetAuthInfo;

namespace Api.Controllers;

[Authorize]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    ///     Sign In
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("signin")]
    [ProducesResponseType(typeof(AuthenticationDto), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Forbidden)]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    ///     Get current authentication info
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(AuthenticationInfoDto), (int) HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetInfo(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAuthInfoRequest(), cancellationToken);
        return Ok(response);
    }
}