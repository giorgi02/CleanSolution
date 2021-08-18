using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Positions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PositionsController> logger;

        public PositionsController(IMediator mediator, ILogger<PositionsController> logger)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpGet]
        public async Task<IEnumerable<GetPositionDto>> Get() =>
            await mediator.Send(new GetPositionQuery.Request());

        [HttpGet("2")]
        public async Task<IActionResult> Get2()
        {
            logger.LogError("test-error");
            return Ok("success");
        }
    }
}
