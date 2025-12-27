using Asp.Versioning;

namespace Presentation.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FakesController(IMediator mediator) : ControllerBase
{
    [HttpGet("employees", Name = "GetFakeEmployees"), SkipResponseLogging]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<GetEmployeeDto>> Get(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetFakeEmployeesQuery.Request(), cancellationToken);
}
