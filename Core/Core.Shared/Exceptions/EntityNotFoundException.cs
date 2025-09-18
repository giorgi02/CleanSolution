using System.Net;

namespace Core.Shared.Exceptions;
/// <summary>
/// მოთხოვნილი ჩანაწერი ვერ მოიძებნა
/// </summary>
[Serializable]
public sealed class EntityNotFoundException(string message, string field = ConstantValues.ExceptionMessage)
    : ApiValidationException(message, field)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public static EntityNotFoundException Create(string message, string field = ConstantValues.ExceptionMessage)
        => new(message, field);
}