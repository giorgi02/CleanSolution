namespace Presentation.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FakesController(IMediator mediator) : ControllerBase
{
    [HttpGet("employees", Name = "GetFakeEmployees"), SkipResponseLogging]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<GetEmployeeDto>> Get(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetFakeEmployeesQuery.Request(), cancellationToken);
}
