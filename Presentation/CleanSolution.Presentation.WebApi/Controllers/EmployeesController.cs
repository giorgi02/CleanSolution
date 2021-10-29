using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Employees.Commands;
using CleanSolution.Core.Application.Features.Employees.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EmployeesController(IMediator mediator) =>
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


        [HttpGet(Name = "GetEmployees")]
        public async Task<IEnumerable<GetEmployeeDto>> Get([FromQuery] GetEmployeesQuery.Request request)
        {
            var result = await _mediator.Send(request);

            Response.Headers.Add("PageIndex", result.PageIndex.ToString());
            Response.Headers.Add("PageSize", result.PageSize.ToString());

            Response.Headers.Add("TotalPages", result.TotalPages.ToString());
            Response.Headers.Add("TotalCount", result.TotalCount.ToString());

            Response.Headers.Add("HasPreviousPage", result.HasPreviousPage.ToString());
            Response.Headers.Add("HasNextPage", result.HasNextPage.ToString());

            return result.Items;
        }

        [HttpGet("{id}", Name = "GetEmployeeById")]
        public async Task<GetEmployeeDto> Get([FromRoute] Guid id) =>
            await _mediator.Send(new GetEmployeeQuery.Request(id));

        [HttpGet("history/{id}", Name = "GetEmployeeHistoryById")]
        public async Task<GetEmployeeDto> GetHistory([FromRoute] Guid id, [FromQuery] int? version, DateTime? actTime, CancellationToken cancellationToken = default) =>
            await _mediator.Send(new GetEmployeeHistoryQuery.Request(id, version, actTime), cancellationToken);


        [HttpPost(Name = "AddEmployee")]
        public async Task<ActionResult> Add([FromForm] CreateEmployeeCommand.Request request, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return CreatedAtRoute("GetEmployeeById", new { id = result }, result);
        }


        /// <summary>
        /// თანამშრომლის კორექტირება
        /// </summary>
        /// <remarks>
        /// Request-ის მაგალითი:
        /// 
        ///     PUT /Employee
        ///     {
        ///         "version": 0,
        ///         "privateNumber": "00000000001",
        ///         "firstName": "Jon",
        ///         "lastName": "Doe",
        ///         "gender": 1,
        ///         "birthDate": "2000-01-01",
        ///         "phones": [
        ///            "123456789"
        ///         ],
        ///         "address": {
        ///            "city": "Foo",
        ///            "street": "Foo #2"
        ///         },
        ///         "positionId": "5b22fe4a-3c07-4e8b-b0d2-3e64f503979c"
        ///     }
        /// </remarks>
        /// <param name="id">5b22fe4a-3c07-4e8b-b0d2-3e64f503979a</param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "UpdateEmployee")]
        //[Authorize(Policy = "EditPolicy")]
        public async Task Update([FromRoute] Guid id, [FromBody] UpdateEmployeeCommand.Request request, CancellationToken cancellationToken = default)
        {
            request.SetId(id);

            await _mediator.Send(request, cancellationToken);
        }


        [HttpDelete("{id}", Name = "DeleteEmployee")]
        [Authorize(Roles = "Administrator")]
        public async Task Delete([FromRoute] Guid id) =>
            await _mediator.Send(new DeleteEmployeeCommand.Request(id));
    }
}
