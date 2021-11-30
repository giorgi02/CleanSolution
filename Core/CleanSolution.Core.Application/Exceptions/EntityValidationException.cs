namespace CleanSolution.Core.Application.Exceptions
{
    public sealed class EntityValidationException : ApplicationBaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public EntityValidationException(IReadOnlyDictionary<string, string[]> errorsDictionary)
            : base("One or more validation errors occurred") => ErrorsDictionary = errorsDictionary;

        public IReadOnlyDictionary<string, string[]> ErrorsDictionary { get; }
    }
}
