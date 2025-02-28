using Asp.Versioning;
using Core.Application.DTOs;
using Core.Application.Interactors.Commands;
using Core.Application.Interactors.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public sealed class TodoItemsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task Create([FromBody] CreateTodoItemCommand.Request request, CancellationToken cancellationToken = default)
        => await mediator.Send(request, cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<GetTodoItemDto?> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
        => await mediator.Send(new GetTodoItemQuery.Request(id), cancellationToken);

    [HttpGet]
    public async Task<IEnumerable<GetTodoItemDto>> Get([FromQuery] GetTodoItemsQuery.Request request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(request, cancellationToken);

        foreach (var item in result.GetParams())
            Response.Headers.Add(item);

        return result.Items;
    }
}