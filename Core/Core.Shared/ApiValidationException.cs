using System.Net;

namespace Core.Shared;

[Serializable]
public class ApiValidationException : Exception
{
    public virtual HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public IDictionary<string, string[]> Messages { get; }


    public ApiValidationException(IDictionary<string, string[]> messages)
            : base("One or more validation errors occurred") => Messages = messages;

    public ApiValidationException(string message, string field = ConstantValues.ExceptionMessage) : base(message)
    {
        Messages = new Dictionary<string, string[]>
        {
            [field] = [message]
        };
    }
}