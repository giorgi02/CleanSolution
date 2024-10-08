using Core.Application.Interactors.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CheckoutsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<CheckoutCommand.Response>> Checkout(CheckoutCommand.Request request, CancellationToken cancellationToken = default)
        => Accepted(await mediator.Send(request, cancellationToken));
}
