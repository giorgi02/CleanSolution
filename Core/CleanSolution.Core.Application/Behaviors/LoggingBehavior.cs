using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CleanSolution.Core.Application.Behaviors;
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IActiveUserService _user;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IActiveUserService user)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("-> request: url={@RequestUrl}, method={@RequestMethod}, type={@type}, body={@body}, userIpAddress={@IpAddress}, userPort={@Port}, userId={@UserId}",
              _user.RequestUrl, _user.RequestMethod, typeof(TRequest).FullName, request, _user.IpAddress, _user.Port, _user.UserId);

        var response = await next();

        _logger.LogInformation("<- response: type={@type}, body={@body}", typeof(TResponse).FullName, ResponseFilter(response));

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