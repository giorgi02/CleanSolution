using Core.Application.Commons;
using Core.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Core.Application.Behaviors;
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    private readonly IActiveUserService _user;
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IActiveUserService user)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("-> request= url: {@RequestUrl}, method: {@RequestMethod}, type: {@Type}, {@Body}, {@IpAddress}, {@Port}, {@Scheme}, {@Host}, {@Path}, {@UserId}, ",
              _user.RequestedUrl, _user.RequestedMethod, typeof(TRequest).FullName, request, _user.IpAddress, _user.Port, _user.Scheme, _user.Host, _user.Path, _user.UserId);

        var response = await next();

        _logger.LogInformation("<- response= type: {@Type}, body: {@Body}", typeof(TResponse).FullName, ResponseFilter(response));

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