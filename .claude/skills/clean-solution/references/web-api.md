# Web API (Presentation.WebApi)

## Contents
1. What is already in place (don't rebuild it)
2. Controller template
3. Routes, status codes, response types
4. Binding: query, body, form + file, file download
5. Long-running commands (202 Accepted)
6. Caching
7. Authentication: JWT (optional) and API key
8. Logging, correlation, documentation
9. Adding a cross-cutting behavior
10. Error response shape

---

## 1. What is already in place (don't rebuild it)

`Program.cs` pipeline - the order matters, keep it:

```
[dev] MapOpenApi().AllowAnonymous(), MapScalarApiReference().AllowAnonymous(), UseCustomSwaggerUI()
UseCorrelationId          x-correlation-id in/out, ambient for downstream HttpClients
UseResponseCompression    gzip (Fastest) / brotli (Optimal), also over HTTPS
UseMiddleware<ExceptionHandler>   every exception → problem details (must wrap everything below)
UseHttpsRedirection, UseStaticFiles, UseRouting
UseIpRateLimiting         rules in "IpRateLimiting" config
UseCors                   default policy from "Cors:Origins" / "Cors:ExposedHeaders"
UseAuthentication, UseAuthorization
MapControllers
MapHealthChecks("/health").AllowAnonymous()   SQL Server + one URL check per external service
```

Global MVC filters (registered in `AddStartup`): `ActionLoggingAttribute` (request/response logging with duration) and
`FluentValidationFilter` (runs the use case `Validator`, throws `ApiValidationException`). API versioning (`Asp.Versioning`,
URL segment), OpenAPI document (`AddOpenApi`, served at `/openapi/v1.json`, UIs at `/scalar/v1` and `/swagger`), Serilog,
`IMemoryCache`, `IActiveUserService` are registered too.

Controllers therefore contain **no** try/catch, no validation code, no logging of requests, no `ModelState` checks, no
business logic - only: bind input, call the handler, choose the success status.

## 2. Controller template

```csharp
using Asp.Versioning;
using Core.Shared;
using Presentation.WebApi.Extensions.Services;

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

    [HttpGet("{id:guid}", Name = "GetEmployeeById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<GetEmployeeDto> Get([FromRoute] Guid id, [FromServices] GetEmployeeQuery.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new GetEmployeeQuery.Request(id), cancellationToken);

    [HttpPost(Name = "AddEmployee")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetEmployeeDto>> Add([FromForm] CreateEmployeeCommand.Request request, IFormFile? picture, [FromServices] CreateEmployeeCommand.Handler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(request.SetPicture(picture?.ToFileContent()), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an employee.
    /// </summary>
    /// <remarks>
    /// Send back the `version` you received; a stale version returns 409.
    ///
    ///     PUT /api/v1/employees/5b22fe4a-3c07-4e8b-b0d2-3e64f503979a
    ///     {
    ///         "version": 0,
    ///         "firstName": "Jon",
    ///         "lastName": "Doe",
    ///         "positionId": "53c161b8-415e-402d-80c1-da798aa3d047"
    ///     }
    /// </remarks>
    [HttpPut("{id:guid}", Name = "UpdateEmployee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<GetEmployeeDto> Update([FromRoute] Guid id, [FromBody] UpdateEmployeeCommand.Request request, [FromServices] UpdateEmployeeCommand.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(request.SetEmployeeId(id), cancellationToken);

    [HttpDelete("{id:guid}", Name = "DeleteEmployee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task Delete([FromRoute] Guid id, [FromServices] DeleteEmployeeCommand.Handler handler, CancellationToken cancellationToken = default)
        => await handler.Handle(new DeleteEmployeeCommand.Request(id), cancellationToken);
}
```

Conventions:
- `public sealed class {Entities}Controller : ControllerBase`, attributes `[ApiController]`, `[ApiVersion("1.0")]`,
  `[Route("api/v{version:apiVersion}/[controller]")]`.
- Handlers come in as **action parameters** with `[FromServices]` (only the handler that action needs is created). Constructor
  injection only for things shared by all actions (`IMemoryCache`).
- Every action ends with `CancellationToken cancellationToken = default` and passes it on.
- Expression-bodied actions (`=> await handler.Handle(...)`) unless the action must pick a status (`CreatedAtAction`, `Accepted`, `File`).
- Every action has a `Name` (`Get{Entities}`, `Get{Entity}ById`, `Add{Entity}`, `Update{Entity}`, `Delete{Entity}`); extra
  routes use kebab-case segments (`"max-by-salary"`, `"{id:guid}/picture"`).
- Ids in routes always have a type constraint: `{id:guid}`.
- A breaking change of a contract gets a new version (`[ApiVersion("2.0")]` on a new controller or action), never an edit of v1.

## 3. Routes, status codes, response types

| Action | Success | Return type |
|---|---|---|
| list | 200 | `Task<Pagination<TDto>>` (or `Task<IEnumerable<TDto>>` for small reference lists) |
| get by id | 200 | `Task<TDto>` - missing record → `EntityNotFoundException` → 400 |
| create | 201 + `Location` | `Task<ActionResult<TDto>>` with `CreatedAtAction(nameof(Get), new { id = result.Id }, result)` |
| update | 200 | `Task<TDto>`; stale version → 409 |
| delete | 200, empty body | `Task` |
| start background work | 202 | `Task<ActionResult<X.Response>>` with `Accepted(response)` |
| file download | 200 | `Task<IActionResult>` with `File(bytes, contentType, fileName)` |

`ProducesResponseType`: the success code, `ValidationProblemDetails` for 400 (and 409 where concurrency applies), `ProblemDetails`
for 500. These feed the OpenAPI document - keep them accurate.

## 4. Binding

- Lists/filters: `[FromQuery] GetXsQuery.Request request` (class-shaped request with settable properties; arrays as
  repeated keys `?languages=1&languages=2`).
- JSON body: `[FromBody] XCommand.Request request`.
- Route id + body: `[FromRoute] Guid id, [FromBody] ... request` → `request.SetXId(id)`.
- Form with a file: `[FromForm] XCommand.Request request, IFormFile? file` → `request.SetFile(file?.ToFileContent())`.
  `IFormFile` never enters Core.Application; `ToFileContent()` (`Extensions/Services/FormFileExtensions.cs`) converts it to
  `FileContent`. Check size/extension in the handler (validators run before the file is attached); put
  `[RequestSizeLimit(...)]` on the action when uploads can be large, and `[SkipRequestLogging]` if the form holds sensitive data.
- File download: a query returns a nested `Response(byte[] FileData, string ContentType, string FileName)` and the action returns
  `File(response.FileData, response.ContentType, response.FileName)`; mark it `[SkipResponseLogging]`.

## 5. Long-running commands (202 Accepted)

When the work must not block the request (sending emails, generating reports), the command enqueues it and returns at once:

```csharp
[HttpPost("checkout", Name = "Checkout")]
[ProducesResponseType(StatusCodes.Status202Accepted)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<CheckoutCommand.Response>> Checkout([FromBody] CheckoutCommand.Request request, [FromServices] CheckoutCommand.Handler handler, CancellationToken cancellationToken = default)
    => Accepted(await handler.Handle(request, cancellationToken));
```

The handler writes to a `ChannelWriter<T>` (or publishes to Kafka) and returns a tracking id; a `BackgroundService` in
`Presentation.WebApi/Workers` processes it. See `integrations.md` → in-process background queue.

## 6. Caching

- Rarely-changing reference data: `IMemoryCache` injected into the controller's primary constructor.
  ```csharp
  public sealed class PositionsController(IMemoryCache cache) : ControllerBase
  {
      private const string PositionsCacheKey = "positions";

      [HttpGet(Name = "GetPositions")]
      public async Task<IEnumerable<GetPositionDto>?> Get([FromServices] GetPositionsQuery.Handler handler, CancellationToken cancellationToken = default) =>
          await cache.GetOrCreateAsync(PositionsCacheKey, async entry =>
          {
              entry.SlidingExpiration = TimeSpan.FromHours(1);
              return await handler.Handle(new GetPositionsQuery.Request(), cancellationToken);
          });
  }
  ```
  Any action that changes the cached data calls `cache.Remove(PositionsCacheKey)` after the handler succeeds.
- Client/proxy caching of public, user-independent GETs: `[ResponseCache(Duration = 60)]`.
- Never cache per-user data under a shared key.

## 7. Authentication

### JWT (optional - off unless the user asks)

`Extensions/Services/JwtValidationExtensions.cs` already contains everything. To enable:
1. In `AddStartup`, replace `builder.Services.AddAuthentication(); builder.Services.AddAuthorization();` with
   `builder.Services.AddJwtAuthenticationConfigs(builder.Configuration);` and `builder.Services.AddJwtAuthorizationConfigs();`.
2. Configure `JwtSettings:Issuer`, `JwtSettings:Audience`; put `JwtSettings:Key` (at least 32 random bytes) in user-secrets /
   environment.
3. The fallback policy now requires an authenticated user on **every** endpoint. Mark public ones `[AllowAnonymous]`
   (`/health`, `/openapi`, `/scalar` already are in the skeleton; in an older solution add `.AllowAnonymous()` to those maps).
4. Fine-grained rules: add policies in `AddJwtAuthorizationConfigs` (`options.AddPolicy("DeletePolicy", p => p.RequireClaim("resources", "delete:user"))`)
   and use `[Authorize(Policy = "DeletePolicy")]` or `[Authorize(Roles = "admin")]` on actions.
5. `IActiveUserService.UserId` (and so the audit columns) comes from the `NameIdentifier` claim.

### API key for internal service-to-service calls

`ApiKeyAuthenticationAttribute` checks header `x-api-key` against config `ApiKey` (constant-time compare; refuses to run with an
empty key). It needs DI, so apply it as `[TypeFilter(typeof(ApiKeyAuthenticationAttribute))]` on the controller or action -
a plain `[ApiKeyAuthentication]` does not compile.

## 8. Logging, correlation, documentation

- `ActionLoggingAttribute` logs every action's `request` argument and the result with status and duration. Use
  `[SkipRequestLogging]` for bodies with secrets/personal data/files and `[SkipResponseLogging]` for large lists and files.
  The action's input parameter must be named `request` to be logged.
- Correlation: `x-correlation-id` is read (or created), echoed in the response, and forwarded by HttpClients that use
  `CorrelationDelegatingHandler`.
- XML comments (`/// <summary>`, `/// <remarks>` with an indented request example) end up in the OpenAPI document
  (`GenerateDocumentationFile` → `wwwroot/EndpointDescriptions.xml`, warning 1591 suppressed). Write them for non-obvious
  endpoints (concurrency, uploads, filters), in English.
- New external dependency → add its health check in `AddStartup`
  (`.AddUrlGroup(new Uri(builder.Configuration.GetString("ExternalServices:X")), name: "x", tags: ["service"])`) using the same
  config key the HttpClient uses. Always pass a unique `name:` - unnamed URL checks share one default name and collide.

## 9. Adding a cross-cutting behavior

- Needs MVC context (action arguments, results, attributes) → an `IAsyncActionFilter` / `ActionFilterAttribute` in
  `Extensions/Attributes` or `Extensions/Services`, registered globally in `AddControllers(options => options.Filters.Add<T>())`
  or applied per action.
- Needs every request including non-MVC endpoints → middleware in `Extensions/Middlewares` (convention class with
  `InvokeAsync(HttpContext)` or an `app.Use(...)` extension like `UseCorrelationId`), placed deliberately in the pipeline.
- Configuration of a framework feature → a private `Add...` method in `Extensions/DependencyInjection.cs` called from `AddStartup`.

## 10. Error response shape

Every failure the client sees has this shape (produced only by `ExceptionHandler`):

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "PositionId": [ "Position was not found." ] },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

409 uses the same shape; 499 for a cancelled request; 500 has `title: "Server Error."` and no internal details (the exception is
in the logs under the same `traceId`).
