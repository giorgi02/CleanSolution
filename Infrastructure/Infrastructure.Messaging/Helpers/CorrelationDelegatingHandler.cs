using System.Diagnostics;

namespace Infrastructure.Messaging.Helpers;
internal class CorrelationDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("x-correlation-id", Trace.CorrelationManager.ActivityId.ToString());

        return base.SendAsync(request, cancellationToken);
    }
}