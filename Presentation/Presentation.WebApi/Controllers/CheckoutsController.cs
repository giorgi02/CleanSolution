using Core.Application.Interactors.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CheckoutsController(IMediator mediator) : ControllerBase
{
    [HttpPost("a1")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<CheckoutCommand1.Response>> Checkout1(CheckoutCommand1.Request request, CancellationToken cancellationToken = default)
        => Accepted(await mediator.Send(request, cancellationToken));

    [HttpPost("a2")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<CheckoutCommand2.Response>> Checkout2(CheckoutCommand2.Request request, CancellationToken cancellationToken = default)
        => Accepted(await mediator.Send(request, cancellationToken));
}
