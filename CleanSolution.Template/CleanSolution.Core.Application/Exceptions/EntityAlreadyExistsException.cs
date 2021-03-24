using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public class EntityAlreadyExistsException : EntityValidationException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public EntityAlreadyExistsException(string message) : base(message) { }
    }
}
