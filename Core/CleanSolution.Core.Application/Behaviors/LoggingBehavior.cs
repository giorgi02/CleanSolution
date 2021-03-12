﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;
        private readonly ICurrentUserService user;
        private readonly IMapper mapper;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService user, IMapper mapper)
        {
            this.logger = logger;
            this.user = user;
            this.mapper = mapper;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            logger.LogInformation("request: method={@RequestMethod}, url={@RequestUrl}, type={@name}, body={@request}, userIpAddress={@IpAddress}, userPort={@Port}, accountType={@AccountType}, acountId={@AccountId}",
                user.RequestMethod, user.RequestUrl, typeof(TRequest).Name, request, user.IpAddress, user.Port, user.AccountType.ToString(), user.AccountId);

            var response = await next();

            //logger.LogInformation("response: method={@RequestMethod}, url={@RequestUrl}, type={@name}, body={@response}, accountType={@AccountType}, acountId={@AccountId}",
            //    user.RequestMethod, user.RequestUrl, typeof(TResponse).Name, logResponse(response), user.AccountType.ToString(), user.AccountId);

            return response;
        }

    }
}
