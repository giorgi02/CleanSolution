using Core.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Behaviors;

public class ValidationBehaviorForReturn<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IValidator<TRequest>? _validator;
    public ValidationBehaviorForReturn(IServiceProvider services) => _validator = services.GetService<IValidator<TRequest>>();


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validator != null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                throw new ApiValidationException(validationResult.ToDictionary());
        }
        return await next();
    }
}