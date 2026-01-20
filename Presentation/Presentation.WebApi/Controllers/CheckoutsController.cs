using Asp.Versioning;

namespace Presentation.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CheckoutsController(IMediator mediator) : ControllerBase
{
    [HttpPost("a1")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CheckoutCommand1.Response>> Checkout1(CheckoutCommand1.Request request, CancellationToken cancellationToken = default)
        => Accepted(await mediator.Send(request, cancellationToken));

    [HttpPost("a2")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CheckoutCommand2.Response>> Checkout2(CheckoutCommand2.Request request, CancellationToken cancellationToken = default)
        => Accepted(await mediator.Send(request, cancellationToken));
}