using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Employees.Commands;
using CleanSolution.Core.Application.Features.Employees.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CleanSolution.Core.Application.Features.Employees.Commands.CreateEmployeeCommand;
using static CleanSolution.Core.Application.Features.Employees.Commands.UpdateEmployeeCommand;
using static CleanSolution.Core.Application.Features.Employees.Queries.GetEmployeesQuery;

namespace CleanSolution.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator mediator;
        public EmployeesController(IMediator mediator) =>
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


        [HttpGet]
        public async Task<IEnumerable<GetEmployeeDto>> Get([FromQuery] GetEmployeesQuery.Request request)
        {
            var result = await mediator.Send(request);

            Response.Headers.Add("PageIndex", result.PageIndex.ToString());
            Response.Headers.Add("PageSize", result.PageSize.ToString());

            Response.Headers.Add("TotalPages", result.TotalPages.ToString());
            Response.Headers.Add("TotalCount", result.TotalCount.ToString());

            Response.Headers.Add("HasPreviousPage", result.HasPreviousPage.ToString());
            Response.Headers.Add("HasNextPage", result.HasNextPage.ToString());

            return result.Items;
        }

        [HttpGet("{id}")]
        public async Task<GetEmployeeDto> Get([FromRoute] Guid id) =>
            await mediator.Send(new GetEmployeeQuery.Request(id));

        [HttpPost]
        public async Task Post([FromForm] CreateEmployeeCommand.Request request) =>
            await mediator.Send(request);

        [HttpPut("{id}")]
        public async Task Put(Guid id, [FromBody] UpdateEmployeeCommand.Request request)
        {
            request.SetId(id);

            await mediator.Send(request);
        }

        [HttpDelete("{id}")]
        public async Task Delete(Guid id) =>
            await mediator.Send(new DeleteEmployeeCommand.Request(id));
    }
}
