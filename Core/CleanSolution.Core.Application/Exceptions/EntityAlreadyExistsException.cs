namespace CleanSolution.Core.Application.Exceptions;
public sealed class EntityAlreadyExistsException : ApplicationBaseException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public EntityAlreadyExistsException(string message) : base(message) { }
}