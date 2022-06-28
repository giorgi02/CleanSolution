using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.WebApi.Extensions.Attributes;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipActionLoggingAttribute : ActionFilterAttribute
{
}
