using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanSolution.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly IMediator mediator;

        public PositionsController(IMediator mediator) =>
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


        [HttpGet]
        public async Task<IEnumerable<GetPositionDto>> Get() =>
            await mediator.Send(new GetPositionQuery.Request());
    }
}
