using Core.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Behaviors;

public class ValidationBehaviorNotReturn<TRequest, _> : IPipelineBehavior<TRequest, Unit> where TRequest : IRequest
{
    private readonly IValidator<TRequest>? _validator;
    public ValidationBehaviorNotReturn(IServiceProvider services) => _validator = services.GetService<IValidator<TRequest>>();


    public async Task<Unit> Handle(TRequest request, RequestHandlerDelegate<Unit> next, CancellationToken cancellationToken)
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