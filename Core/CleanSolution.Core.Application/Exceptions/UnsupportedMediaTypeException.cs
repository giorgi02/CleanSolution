using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public class UnsupportedMediaTypeException : EntityValidationException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.UnsupportedMediaType;

        public UnsupportedMediaTypeException(string message) : base(message) { }
    }
}
