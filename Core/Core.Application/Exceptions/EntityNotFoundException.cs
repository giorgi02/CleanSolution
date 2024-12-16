using Core.Application.Commons;

namespace Core.Application.Exceptions;
/// <summary>
/// მოთხოვნილი ჩანაწერი ვერ მოიძებნა
/// </summary>
[Serializable]
public sealed class EntityNotFoundException(string message, string field = ConstantValues.ExceptionMessage)
    : ApiValidationException(message, field)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}