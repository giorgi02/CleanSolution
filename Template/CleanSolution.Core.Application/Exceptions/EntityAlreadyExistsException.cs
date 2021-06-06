using System.Net;

namespace $safeprojectname$.Exceptions
{
    public class EntityAlreadyExistsException : ApplicationBaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public EntityAlreadyExistsException(string message) : base(message) { }
    }
}
