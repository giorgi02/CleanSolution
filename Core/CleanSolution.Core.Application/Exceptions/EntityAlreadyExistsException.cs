using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public class EntityAlreadyExistsException : ApplicationBaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public EntityAlreadyExistsException(string message) : base(message) { }
    }
}
