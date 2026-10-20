using System.Net;

namespace Core.Shared.Exceptions;

/// <summary>
/// The requested record does not exist.
/// Returns 400 on purpose (not 404): 404 must only ever mean "this URL does not exist",
/// so clients can tell a wrong route apart from a missing record.
/// </summary>
[Serializable]
public sealed class EntityNotFoundException(string message, string field = ConstantValues.ExceptionMessage)
    : ApiValidationException(message, field)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public static EntityNotFoundException Create(string message, string field = ConstantValues.ExceptionMessage)
        => new(message, field);
}
