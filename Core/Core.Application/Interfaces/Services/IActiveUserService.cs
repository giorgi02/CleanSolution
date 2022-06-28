namespace Core.Application.Interfaces.Services;
public interface IActiveUserService
{
    Guid? UserId { get; }
    string? IpAddress { get; }
    int Port { get; }

    public string? Scheme { get; }
    public string? Host { get; }
    public string? Path { get; }

    string? RequestedUrl { get; }
    string? RequestedMethod { get; }
}