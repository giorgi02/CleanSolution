using System.Net;

namespace $safeprojectname$.Exceptions
{
    public class ServiceUnavailableException : ApplicationBaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.ServiceUnavailable;

        public ServiceUnavailableException(string message) : base(message) { }
    }
}
