namespace Core.Application.Interactors;

// Only fixes the Handle signature. There is no mediator, dispatcher or pipeline:
// callers (controllers, consumers, workers, MCP tools) get the concrete Handler from DI and call Handle directly.

public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    Task Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
