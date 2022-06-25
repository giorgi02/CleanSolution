namespace Core.Application.Exceptions;
public sealed class DataObsoleteException : ApplicationBaseException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    /// <summary>
    /// ძირითადად გამოიყენება კონკურენტული მოთხოვნებისთვის
    /// </summary>
    public DataObsoleteException(string message) : base(message) { }
}