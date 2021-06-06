using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Workabroad.Presentation.Admin.Extensions.Middlewares
{
    public static class RequestLocalizationMiddleware
    {
        public static void UseRequestLocalizationMiddleware(this IApplicationBuilder app)
        {
            app.UseRequestLocalization(
                options =>
                {
                    options.SupportedCultures = new List<CultureInfo>()
                    {
                        new CultureInfo("en-US"),
                    };

                    options.SupportedUICultures = new List<CultureInfo>()
                    {
                        new CultureInfo("ka-GE"),
                        new CultureInfo("en-US"),
                    };
                    options.DefaultRequestCulture = new RequestCulture("en-US", "ka-GE");
                }
            );
        }
    }

    public class LocalizationMiddleware
    {
        private readonly RequestDelegate next;
        public LocalizationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var currentCulture = context.Request.Headers["Accept-Language"].ToString().Split(',').FirstOrDefault();
            var defaultCulture = "en";

            if (!string.IsNullOrWhiteSpace(currentCulture))
            {
                var checkCulture = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Any(p => string.Equals(p.Name, currentCulture, StringComparison.CurrentCultureIgnoreCase));

                if (!checkCulture) currentCulture = defaultCulture;
            }
            else
            {
                currentCulture = defaultCulture;
            }

            var cultureInfo = new CultureInfo(currentCulture);

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            await next(context);
        }
    }
}
