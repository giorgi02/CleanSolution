﻿using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanSolution.Presentation.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public sealed class PositionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PositionsController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


    [HttpGet(Name = "GetPositions")]
    public async Task<IEnumerable<GetPositionDto>> Get(CancellationToken cancellationToken = default) =>
        await _mediator.Send(new GetPositionQuery.Request(), cancellationToken);
}