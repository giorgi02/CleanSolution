using System.Diagnostics;

namespace Infrastructure.Messaging.Helpers;

/// <summary>
/// Forwards the current correlation id (set by CorrelationMiddleware) to downstream HTTP services.
/// </summary>
internal sealed class CorrelationDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation("x-correlation-id", Trace.CorrelationManager.ActivityId.ToString());

        return base.SendAsync(request, cancellationToken);
    }
}
