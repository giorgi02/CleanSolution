namespace CleanSolution.Core.Application.Behaviors;
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
       where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators) =>
        this.validators = validators;


    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }
        return await next();
    }
    //todo: შევადარო ეს ორი მეთოდი
    //public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    //{
    //    if (!validators.Any())
    //    {
    //        return await next();
    //    }
    //    var context = new ValidationContext<TRequest>(request);
    //    var errorsDictionary = validators
    //        .Select(x => x.Validate(context))
    //        .SelectMany(x => x.Errors)
    //        .Where(x => x != null)
    //        .GroupBy(
    //            x => x.PropertyName,
    //            x => x.ErrorMessage,
    //            (propertyName, errorMessages) => new
    //            {
    //                Key = propertyName,
    //                Values = errorMessages.Distinct().ToArray()
    //            })
    //        .ToDictionary(x => x.Key, x => x.Values);
    //    if (errorsDictionary.Any())
    //    {
    //        throw new CustomeValidationException(errorsDictionary);
    //    }
    //    return await next();
    //}
}