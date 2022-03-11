namespace CleanSolution.Core.Application.Exceptions;
/// <summary>
/// Fluent Validation -ის იმიტაცია
/// </summary>
public sealed class EntityValidationException : ApplicationBaseException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    /// <summary>
    /// Fluent Validation -ის იმიტაცია
    /// </summary>
    public EntityValidationException(IReadOnlyDictionary<string, string[]> errors)
            : base("One or more validation errors occurred") => this.Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
