using Core.Application.DTOs;
using Core.Application.Interactors.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Presentation.WebApi.Controllers;
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class PositionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;

    public PositionsController(IMediator mediator, IMemoryCache cache)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    [HttpGet(Name = "GetPositions")]
    public async Task<IEnumerable<GetPositionDto>?> Get(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync("positions", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await _mediator.Send(new GetPositionsQuery.Request(), cancellationToken);
        });

    [HttpGet("{id:guid}", Name = "GetPosition")]
    public async Task<GetPositionDto?> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
        => await _mediator.Send(new GetPositionQuery.Request(id), cancellationToken);
}