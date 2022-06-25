namespace Core.Application.Exceptions;
public sealed class OperationRejectedException : ApplicationBaseException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotAcceptable;

    /// <summary>
    /// მსგავსი მოქმედება აკრძალულია
    /// </summary>
    public OperationRejectedException(string message) : base(message) { }
}