namespace Core.Application.Exceptions;
/// <summary>
/// მოთხოვნილი ჩანაწერი ვერ მოიძებნა
/// </summary>
[Serializable]
public sealed class EntityNotFoundException(string message, string field = "messages")
    : EntityValidationException(message, field)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}