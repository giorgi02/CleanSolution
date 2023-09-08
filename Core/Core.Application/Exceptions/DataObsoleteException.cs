namespace Core.Application.Exceptions;
public sealed class DataObsoleteException : EntityValidationException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    /// <summary>
    /// ძირითადად გამოიყენება კონკურენტული მოთხოვნებისთვის
    /// </summary>
    public DataObsoleteException(string message, string field = "messages") : base(message, field) { }
}