using System;
using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public abstract class ApplicationBaseException : Exception
    {
        public abstract HttpStatusCode StatusCode { get; }

        public ApplicationBaseException(string message) : base(message) { }
    }
}
