using $safeprojectname$.Interfaces.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace $safeprojectname$.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
            logger.LogInformation("request: url={@RequestUrl}, method={@RequestMethod}, type={@name}, body={@request}, userIpAddress={@IpAddress}, userPort={@Port}, userId={@UserId}",
              user.RequestUrl, user.RequestMethod, typeof(TRequest).Name, request, user.IpAddress, user.Port, user.UserId);


            var response = await next();

            //logger.LogInformation("response: method={@RequestMethod}, url={@RequestUrl}, type={@name}, body={@response}, accountType={@AccountType}, acountId={@AccountId}",
            //    user.RequestMethod, user.RequestUrl, typeof(TResponse).Name, logResponse(response), user.AccountType.ToString(), user.AccountId);

            return response;
        }

    }
}
