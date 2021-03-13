using System;

namespace CleanSolution.Core.Application.Interfaces.Contracts
{
    public interface ICurrentUserService
    {
        public Guid AccountId { get; }
        public string IpAddress { get; }
        public int Port { get; }
        public string RequestUrl { get; }
        public string RequestMethod { get; }
    }
}
