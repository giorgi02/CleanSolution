namespace CleanSolution.Core.Application.Interfaces.Contracts;
public interface IActiveUserService
{
    Guid UserId { get; }
    string IpAddress { get; }
    int Port { get; }
    string RequestUrl { get; }
    string RequestMethod { get; }
}