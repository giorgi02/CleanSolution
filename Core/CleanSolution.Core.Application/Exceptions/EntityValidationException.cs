using System;
using System.Net;

namespace Workabroad.Core.Application.Exceptions
{
    public abstract class EntityValidationException : Exception
    {
        public abstract HttpStatusCode StatusCode { get; }

        public EntityValidationException(string message) : base(message) { }
    }
}
