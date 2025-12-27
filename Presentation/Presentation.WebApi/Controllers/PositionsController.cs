using Asp.Versioning;

namespace Presentation.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PositionsController(IMediator mediator, IMemoryCache cache) : ControllerBase
{
    [HttpGet(Name = "GetPositions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async ValueTask<IEnumerable<GetPositionDto>?> Get(CancellationToken cancellationToken = default) =>
        await cache.GetOrCreateAsync("positions", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await mediator.Send(new GetPositionsQuery.Request(), cancellationToken);
        });

    [HttpGet("{id:guid}", Name = "GetPosition")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<GetPositionDto?> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
        => await mediator.Send(new GetPositionQuery.Request(id), cancellationToken);

    [ResponseCache(Duration = 60)]
    [HttpGet("max-by-salary", Name = "GetMaxBySalary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async ValueTask<GetPositionDto?> GetMaxBySalary(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetMaxMinBySalaryQuery.Request(true), cancellationToken);

    [ResponseCache(Duration = 60)]
    [HttpGet("min-by-salary", Name = "GetMinBySalary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async ValueTask<GetPositionDto?> GetMinBySalary(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetMaxMinBySalaryQuery.Request(false), cancellationToken);
}