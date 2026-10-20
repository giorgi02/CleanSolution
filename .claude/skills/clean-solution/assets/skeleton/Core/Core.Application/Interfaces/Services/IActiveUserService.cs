namespace Core.Application.Interfaces.Services;

/// <summary>
/// Who is calling and from where. Implemented per host:
/// ActiveUserService (WebApi, from HttpContext) and SystemActiveUserService (Worker / MCP / consumers).
/// </summary>
public interface IActiveUserService
{
    string? UserId { get; }
    string? IpAddress { get; }
    int Port { get; }

    string? Scheme { get; }
    string? Host { get; }
    string? Path { get; }

    string? RequestedUrl { get; }
    string? RequestedMethod { get; }
}
