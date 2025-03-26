namespace Presentation.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class FakesController(IMediator mediator) : ControllerBase
{
    [HttpGet("employees", Name = "GetFakeEmployees"), SkipResponseLogging]
    public async Task<IEnumerable<GetEmployeeDto>> Get(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetFakeEmployeesQuery.Request(), cancellationToken);
}
