namespace Core.Application.Commons;
public static class ResponseFactory
{
    public static object CreateSuccess(bool isSuccess, params string[] messages) => new
    {
        isSuccess,
        messages
    };

    public static Failure CreateFailure(string? traceId) => new(traceId);


    public class Failure
    {
        public string Type { get; }
        public string Title { get; private set; }
        public int Status { get; private set; }
        public string? TraceId { get; private set; }
        public IDictionary<string, string[]> Errors { get; private set; }

        public Failure(string? traceId)
        {
            this.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            this.Title = "One or more validation errors occurred.";
            this.Status = (int)HttpStatusCode.BadRequest;
            this.TraceId = traceId;
            this.Errors = new Dictionary<string, string[]>(1);
        }

        public Failure SetTitle(string title)
        {
            this.Title = title;
            return this;
        }

        public Failure SetStatus(HttpStatusCode status)
        {
            this.Status = (int)status;
            return this;
        }

        public Failure SetErrors(IReadOnlyDictionary<string, string[]> errors)
        {
            this.Errors = new Dictionary<string, string[]>(errors);
            return this;
        }

        public Failure SetErrors(string errors)
        {
            this.Errors.TryAdd("messages", new[] { errors });
            return this;
        }
    }
}