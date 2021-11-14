using System.Net;

namespace CleanSolution.Core.Application.Exceptions;
public class EntityAlreadyExistsException : ApplicationBaseException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public EntityAlreadyExistsException(string message) : base(message) { }
}