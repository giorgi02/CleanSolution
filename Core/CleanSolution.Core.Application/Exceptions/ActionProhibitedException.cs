using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public class ActionProhibitedException : EntityValidationException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.NotAcceptable;

        /// <summary>
        /// მსგავსი მონაცემის არსებობა ბაზაში აკრძალულია
        /// მსგავსი მოქმედება აკრძალულია
        /// </summary>
        public ActionProhibitedException(string message) : base(message) { }
    }
}
