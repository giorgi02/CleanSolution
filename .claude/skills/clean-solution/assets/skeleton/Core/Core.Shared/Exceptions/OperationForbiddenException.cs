using System.Net;

namespace Core.Shared.Exceptions;

/// <summary>
/// The operation conflicts with the current state of the record (409).
/// Mainly used for optimistic-concurrency conflicts: the Version sent by the client is outdated.
/// </summary>
[Serializable]
public sealed class OperationForbiddenException : ApiValidationException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    private OperationForbiddenException(string message, string field) : base(message, field) { }
    public static OperationForbiddenException Create(string message, string field = ConstantValues.ExceptionMessage)
        => new(message, field);
}
