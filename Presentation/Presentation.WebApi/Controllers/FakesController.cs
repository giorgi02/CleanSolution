using Asp.Versioning;

namespace Presentation.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FakesController : ControllerBase
{
    [HttpGet("employees", Name = "GetFakeEmployees"), SkipResponseLogging]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<GetEmployeeDto>> Get([FromServices] GetFakeEmployeesQuery.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new GetFakeEmployeesQuery.Request(), cancellationToken);
}
