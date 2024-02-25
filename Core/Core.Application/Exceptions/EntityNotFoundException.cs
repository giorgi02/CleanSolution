using System.Runtime.Serialization;

namespace Core.Application.Exceptions;
[Serializable]
public sealed class EntityNotFoundException : EntityValidationException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    /// <summary>
    /// მოთხოვნილი ჩანაწერი ვერ მოიძებნა
    /// </summary>
    public EntityNotFoundException(string message, string field = "messages")
        : base(message, field) { }

    private EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }
}