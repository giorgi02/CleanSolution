using Asp.Versioning;
using Core.Shared;

namespace Presentation.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class EmployeesController : ControllerBase
{
    [HttpGet(Name = "GetEmployees"), SkipResponseLogging]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<Pagination<GetEmployeeDto>> Get([FromQuery] GetEmployeesQuery.Request request, [FromServices] GetEmployeesQuery.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(request, cancellationToken);

    [HttpGet("{id}", Name = "GetEmployeeById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<GetEmployeeDto> Get([FromRoute] Guid id, [FromServices] GetEmployeeQuery.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new GetEmployeeQuery.Request(id), cancellationToken);

    [HttpPost(Name = "AddEmployee")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetEmployeeDto>> Add([FromForm] CreateEmployeeCommand.Request request, [FromServices] CreateEmployeeCommand.Handler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(request, cancellationToken);
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
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id}", Name = "UpdateEmployee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<GetEmployeeDto> Update([FromRoute] Guid id, [FromBody] UpdateEmployeeCommand.Request request, [FromServices] UpdateEmployeeCommand.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(request.SetEmployeeId(id), cancellationToken);

    //// todo: ეს მეთოდი ბოლომდე დავამუშაო
    //[HttpPatch("{id}", Name = "EditEmployee")]
    //public async Task Edit([FromRoute] Guid id, [FromBody] JsonPatchDocument<EditEmployeeCommand.Request> request, [FromServices] EditEmployeeCommand.Handler handler, CancellationToken cancellationToken = default)
    //{
    //    await handler.Handle(request, cancellationToken);
    //}

    //[Authorize(Policy = "DeletePolicy")]
    //[Authorize(Roles = "admin, editor")]
    [HttpDelete("{id}", Name = "DeleteEmployee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task Delete([FromRoute] Guid id, [FromServices] DeleteEmployeeCommand.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new DeleteEmployeeCommand.Request(id), cancellationToken);
}