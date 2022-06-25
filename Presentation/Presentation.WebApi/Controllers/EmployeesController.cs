using Core.Application.DTOs;
using Core.Application.Features.Employees.Commands;
using Core.Application.Features.Employees.Queries;
using Core.Domain.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public sealed class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    public EmployeesController(IMediator mediator) =>
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


    [HttpGet(Name = "GetEmployees")]
    public async Task<IEnumerable<GetEmployeeDto>> Get([FromQuery] GetEmployeesQuery.Request request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);

        result.GetParams().ForEach(param => Response.Headers.Add(param));

        return result.Items;
    }

    [HttpGet("{id}", Name = "GetEmployeeById")]
    public async Task<GetEmployeeDto> Get([FromRoute] Guid id, CancellationToken cancellationToken = default) =>
        await _mediator.Send(new GetEmployeeQuery.Request(id), cancellationToken);

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
    //[Authorize(Roles = "editor, admin")]
    public async Task<GetEmployeeDto> Update([FromRoute] Guid id, [FromBody] UpdateEmployeeCommand.Request request, CancellationToken cancellationToken = default)
    {
        request.SetId(id);

        return await _mediator.Send(request, cancellationToken);
    }
    //// todo: ეს მეთოდი ბოლომდე დავამუშაო
    //[HttpPatch("{id}", Name = "EditEmployee")]
    //public async Task Edit([FromRoute] Guid id, [FromBody] JsonPatchDocument<EditEmployeeCommand.Request> request, CancellationToken cancellationToken = default)
    //{
    //    await _mediator.Send(request, cancellationToken);
    //}

    [HttpDelete("{id}", Name = "DeleteEmployee")]
    [Authorize(Roles = "admin")]
    public async Task Delete([FromRoute] Guid id) =>
        await _mediator.Send(new DeleteEmployeeCommand.Request(id));
}