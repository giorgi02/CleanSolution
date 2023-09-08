namespace Core.Application.Exceptions;
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
}