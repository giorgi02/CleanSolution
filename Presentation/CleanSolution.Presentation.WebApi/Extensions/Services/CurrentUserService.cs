using CleanSolution.Core.Application.Interfaces.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace CleanSolution.Presentation.WebApi.Extensions.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        /// <summary>
        /// იუზერის მონაცემების და მოთხოვნის ინფორმაციის ამოღება
        /// საჭიროა ორივე კონსტრუქტორი: პირველი IoC კონტეინერისთვის გამოიყენება, მეორე ხელიტ შექმნისთვის.
        /// </summary>
        public CurrentUserService(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor.HttpContext) { }
        public CurrentUserService(HttpContext context)
        {
            if (context == null) return;

            this.AccountId = Guid.TryParse(context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out Guid result) ? result : Guid.Empty;
            this.IpAddress = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
            this.Port = context.Connection?.RemotePort ?? 0;

            this.RequestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            this.RequestMethod = context.Request.Method;
        }


        public Guid AccountId { get; }
        public string IpAddress { get; }
        public int Port { get; }

        public string RequestUrl { get; }
        public string RequestMethod { get; }
    }
}
