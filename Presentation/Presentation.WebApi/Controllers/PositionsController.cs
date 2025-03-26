using Asp.Versioning;

namespace Presentation.WebApi.Controllers;
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class PositionsController(IMediator mediator, IMemoryCache cache) : ControllerBase
{
    [HttpGet(Name = "GetPositions")]
    public async ValueTask<IEnumerable<GetPositionDto>?> Get(CancellationToken cancellationToken = default) =>
        await cache.GetOrCreateAsync("positions", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await mediator.Send(new GetPositionsQuery.Request(), cancellationToken);
        });

    [HttpGet("{id:guid}", Name = "GetPosition")]
    public async Task<GetPositionDto?> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
        => await mediator.Send(new GetPositionQuery.Request(id), cancellationToken);

    [ResponseCache(Duration = 60)]
    [HttpGet("max-by-salary", Name = "GetMaxBySalary")]
    public async ValueTask<GetPositionDto?> GetMaxBySalary(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetMaxMinBySalaryQuery.Request(true), cancellationToken);

    [ResponseCache(Duration = 60)]
    [HttpGet("min-by-salary", Name = "GetMinBySalary")]
    public async ValueTask<GetPositionDto?> GetMinBySalary(CancellationToken cancellationToken = default)
        => await mediator.Send(new GetMaxMinBySalaryQuery.Request(false), cancellationToken);
}