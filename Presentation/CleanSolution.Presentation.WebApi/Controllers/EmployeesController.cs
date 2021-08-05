using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Employees.Commands;
using CleanSolution.Core.Application.Features.Employees.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet("history/{id}")]
        public async Task<GetEmployeeDto> GetHistory([FromRoute] Guid id, [FromQuery] int? version, DateTime? actTime) =>
            await mediator.Send(new GetEmployeeHistoryQuery.Request(id, version, actTime));

        [HttpPost]
        public async Task Post([FromForm] CreateEmployeeCommand.Request request) =>
            await mediator.Send(request);

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
        /// <returns></returns>
        [HttpPut("{id}")]
        //[Authorize(Policy = "EditPolicy")]
        public async Task Put(Guid id, [FromBody] UpdateEmployeeCommand.Request request)
        {
            request.SetId(id);

            await mediator.Send(request);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task Delete(Guid id) =>
            await mediator.Send(new DeleteEmployeeCommand.Request(id));
    }
}
