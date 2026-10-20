using System.Net;

namespace Core.Shared.Exceptions;

/// <summary>
/// Base of all expected (client-caused) errors. ExceptionHandler turns it into a ValidationProblemDetails
/// response with <see cref="StatusCode"/> as status and <see cref="Messages"/> as the errors dictionary.
/// </summary>
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
