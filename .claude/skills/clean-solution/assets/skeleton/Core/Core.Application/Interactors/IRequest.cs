namespace Core.Application.Interactors;

// Marker interfaces: they do nothing at runtime, they only tie a request to its response type.

public interface IRequest { }

public interface IRequest<out TResponse> { }
