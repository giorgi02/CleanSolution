using Core.Application.Interfaces.Services;

namespace Core.Application.Commons;

/// <summary>
/// <see cref="IActiveUserService"/> for hosts that have no HTTP request (Worker, MCP server, Kafka consumers).
/// Audit columns then record the change as made by "system".
/// </summary>
public sealed class SystemActiveUserService : IActiveUserService
{
    public string? UserId => "system";
    public string? IpAddress => null;
    public int Port => 0;

    public string? Scheme => null;
    public string? Host => null;
    public string? Path => null;

    public string? RequestedUrl => null;
    public string? RequestedMethod => null;
}
