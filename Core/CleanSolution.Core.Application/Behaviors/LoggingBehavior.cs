using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.Extensions.Logging;

namespace CleanSolution.Core.Application.Behaviors;
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;
    private readonly IActiveUserService user;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IActiveUserService user)
    {
        this.logger = logger;
        this.user = user;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        logger.LogInformation("=> request: url={@RequestUrl}, method={@RequestMethod}, type={@type}, body={@body}, userIpAddress={@IpAddress}, userPort={@Port}, userId={@UserId}",
             user.RequestUrl, user.RequestMethod, typeof(TRequest).FullName, request, user.IpAddress, user.Port, user.UserId);

        var response = await next();

        logger.LogInformation("<= response: type={@type}, body={@body}", typeof(TResponse).FullName, ResponseFilter(response));

        return response;
    }

    // თუ პასუხი მასივია, მთელი ობიექტი რომ არ დალოგირდეს, ტოვებს მხოლოდ მცირე ნაწილს
    private static object? ResponseFilter(TResponse response)
    {
        if (typeof(Pagination<>).Name == typeof(TResponse).Name)
        {
            dynamic res = response!;

            return new
            {
                Items = default(IList<object>),

                res.PageIndex,
                res.PageSize,

                res.TotalPages,
                res.TotalCount,

                res.HasPreviousPage,
                res.HasNextPage
            };
        }
        else
        {
            return response;
        }
    }
}