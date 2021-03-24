using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public class ServiceUnavailableException : EntityValidationException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.ServiceUnavailable;

        public ServiceUnavailableException(string message) : base(message) { }
    }
}
