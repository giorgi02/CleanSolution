using Core.Application.Commons;

namespace Core.Application.Exceptions;
/// <summary>
/// ძირითადად გამოიყენება კონკურენტული მოთხოვნებისთვის
/// </summary>
[Serializable]
public sealed class OperationForbiddenException : ApiValidationException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    private OperationForbiddenException(string message, string field) : base(message, field) { }
    public static OperationForbiddenException Create(string message, string field = ConstantValues.ExceptionMessage)
        => new(message, field);
}