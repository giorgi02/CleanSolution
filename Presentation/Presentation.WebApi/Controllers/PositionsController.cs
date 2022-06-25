using Core.Application.DTOs;
using Core.Application.Features.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Presentation.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
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
    public async Task<IEnumerable<GetPositionDto>> Get(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync("positions", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await _mediator.Send(new GetPositionQuery.Request(), cancellationToken);
        });
}