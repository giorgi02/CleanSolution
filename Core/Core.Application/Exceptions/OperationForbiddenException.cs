using System.Runtime.Serialization;

namespace Core.Application.Exceptions;
[Serializable]
public sealed class OperationForbiddenException : EntityValidationException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    /// <summary>
    /// ძირითადად გამოიყენება კონკურენტული მოთხოვნებისთვის
    /// </summary>
    public OperationForbiddenException(string message, string field = "messages")
        : base(message, field) { }

    private OperationForbiddenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }
}