using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Commons;

public static class ValidationExtensions
{
    /// <summary>
    /// Runs the validator registered for <typeparamref name="TRequest"/> (if there is one) and throws
    /// <see cref="ApiValidationException"/> when the request is invalid.
    /// HTTP requests are validated by FluentValidationFilter; call this from every other entry point
    /// (Kafka consumers, background workers, MCP tools) before calling the handler.
    /// </summary>
    public static async Task ValidateOrThrowAsync<TRequest>(this IServiceProvider services, TRequest request, CancellationToken cancellationToken = default)
    {
        var validator = services.GetService<IValidator<TRequest>>();
        if (validator is null) return;

        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new ApiValidationException(result.ToDictionary());
    }
}
