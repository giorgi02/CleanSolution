using System;

namespace $safeprojectname$.Interfaces.Contracts
{
    public interface IActiveUserService
    {
        public Guid UserId { get; }
        public string IpAddress { get; }
        public int Port { get; }
        public string RequestUrl { get; }
        public string RequestMethod { get; }
    }
}
