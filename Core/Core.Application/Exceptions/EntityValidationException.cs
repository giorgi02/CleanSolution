using System.Runtime.Serialization;

namespace Core.Application.Exceptions;
[Serializable]
public class EntityValidationException : Exception
{
    public virtual HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public IDictionary<string, string[]> Messages { get; }


    public EntityValidationException(IDictionary<string, string[]> messages)
            : base("One or more validation errors occurred") => this.Messages = messages;

    public EntityValidationException(string message, string field = "messages") : base(message)
    {
        this.Messages = new Dictionary<string, string[]>
        {
            { field, new[] { message } }
        };
    }

    protected EntityValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Messages = (IDictionary<string, string[]>)info.GetValue(nameof(Messages), typeof(IDictionary<string, string[]>))!;
    }
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue(nameof(Messages), this.Messages, typeof(IDictionary<string, string[]>));
    }
}