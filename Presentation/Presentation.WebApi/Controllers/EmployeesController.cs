namespace Presentation.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public sealed class EmployeesController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetEmployees"), SkipResponseLogging]
    public async Task<Pagination<GetEmployeeDto>> Get([FromQuery] GetEmployeesQuery.Request request, CancellationToken cancellationToken = default)
        => await mediator.Send(request, cancellationToken);

    [HttpGet("{id}", Name = "GetEmployeeById")]
    public async Task<GetEmployeeDto> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
        => await mediator.Send(new GetEmployeeQuery.Request(id), cancellationToken);

    [HttpPost(Name = "AddEmployee")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<GetEmployeeDto>> Add([FromForm] CreateEmployeeCommand.Request request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
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
    public async Task<GetEmployeeDto> Update([FromRoute] Guid id, [FromBody] UpdateEmployeeCommand.Request request, CancellationToken cancellationToken = default)
        => await mediator.Send(request.SetEmployeeId(id), cancellationToken);

    //// todo: ეს მეთოდი ბოლომდე დავამუშაო
    //[HttpPatch("{id}", Name = "EditEmployee")]
    //public async Task Edit([FromRoute] Guid id, [FromBody] JsonPatchDocument<EditEmployeeCommand.Request> request, CancellationToken cancellationToken = default)
    //{
    //    await _mediator.Send(request, cancellationToken);
    //}

    //[Authorize(Policy = "DeletePolicy")]
    //[Authorize(Roles = "admin, editor")]
    [HttpDelete("{id}", Name = "DeleteEmployee")]
    public async Task Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        => await mediator.Send(new DeleteEmployeeCommand.Request(id), cancellationToken);
}