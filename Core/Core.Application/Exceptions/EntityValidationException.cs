using Core.Application.Commons;

namespace Core.Application.Exceptions;
[Serializable]
public class EntityValidationException : Exception
{
    public virtual HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public IDictionary<string, string[]> Messages { get; }


    public EntityValidationException(IDictionary<string, string[]> messages)
            : base("One or more validation errors occurred") => this.Messages = messages;

    public EntityValidationException(string message, string field = ConstantValues.ExceptionMessage) : base(message)
    {
        this.Messages = new Dictionary<string, string[]>
        {
            { field, new[] { message } }
        };
    }
}