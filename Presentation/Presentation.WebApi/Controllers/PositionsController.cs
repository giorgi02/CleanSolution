using Asp.Versioning;

namespace Presentation.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PositionsController(IMemoryCache cache) : ControllerBase
{
    [HttpGet(Name = "GetPositions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async ValueTask<IEnumerable<GetPositionDto>?> Get([FromServices] GetPositionsQuery.Handler handler, CancellationToken cancellationToken = default) =>
        await cache.GetOrCreateAsync("positions", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await handler.Handle(new GetPositionsQuery.Request(), cancellationToken);
        });

    [HttpGet("{id:guid}", Name = "GetPosition")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<GetPositionDto?> Get([FromRoute] Guid id, [FromServices] GetPositionQuery.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new GetPositionQuery.Request(id), cancellationToken);

    [ResponseCache(Duration = 60)]
    [HttpGet("max-by-salary", Name = "GetMaxBySalary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async ValueTask<GetPositionDto?> GetMaxBySalary([FromServices] GetMaxMinBySalaryQuery.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new GetMaxMinBySalaryQuery.Request(true), cancellationToken);

    [ResponseCache(Duration = 60)]
    [HttpGet("min-by-salary", Name = "GetMinBySalary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async ValueTask<GetPositionDto?> GetMinBySalary([FromServices] GetMaxMinBySalaryQuery.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new GetMaxMinBySalaryQuery.Request(false), cancellationToken);
}