using System.Net;

namespace CleanSolution.Core.Application.Exceptions
{
    public class ActionProhibitedException : ApplicationBaseException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.NotAcceptable;

        /// <summary>
        /// მსგავსი მოქმედება აკრძალულია
        /// </summary>
        public ActionProhibitedException(string message) : base(message) { }
    }
}
