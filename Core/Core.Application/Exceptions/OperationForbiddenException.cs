using Core.Application.Commons;

namespace Core.Application.Exceptions;
/// <summary>
/// ძირითადად გამოიყენება კონკურენტული მოთხოვნებისთვის
/// </summary>
[Serializable]
public sealed class OperationForbiddenException(string message, string field = ConstantValues.ExceptionMessage)
    : ApiValidationException(message, field)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}