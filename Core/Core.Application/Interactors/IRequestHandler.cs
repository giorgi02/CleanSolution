namespace Core.Application.Interactors;

// მხოლოდ Handle მეთოდის სიგნატურას აფიქსირებს — dispatch/pipeline არ არსებობს, handler-ს პირდაპირ იძახებენ

public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    Task Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
