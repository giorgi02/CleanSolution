namespace CleanSolution.Core.Application.Exceptions;
public sealed class ManyValidationException : ApplicationBaseException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    // todo:  შესაძლებელია ამ exception ის უკეთ დამუშავება
    public ManyValidationException(params string[] messages)
        : base(string.Join("; ", messages)) { }
}