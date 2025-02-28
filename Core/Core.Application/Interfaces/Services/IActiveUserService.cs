namespace Core.Application.Interfaces.Services;
public interface IActiveUserService
{
    string? UserId { get; }
    string? IpAddress { get; }
}