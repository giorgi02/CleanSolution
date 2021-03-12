using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public class UnAuthenticatedException : EntityValidationException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;

        public UnAuthenticatedException(string message) : base(message) { }
    }
}
