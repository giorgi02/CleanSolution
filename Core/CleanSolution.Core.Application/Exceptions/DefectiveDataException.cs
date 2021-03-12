using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public class DefectiveDataException : EntityValidationException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.UnprocessableEntity;

        /// <summary>
        /// ბაზაში აღმოჩენილია დეფექტური ჩანაწერი
        /// </summary>
        public DefectiveDataException(string message) : base(message) { }
    }
}
