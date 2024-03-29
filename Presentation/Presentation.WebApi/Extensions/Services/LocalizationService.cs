﻿using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Presentation.WebApi.Extensions.Services;
public static class LocalizationService
{
    /// <summary>
    /// გამოძახება: Headers["Accept-Language"] = "ka-GE"
    /// </summary>
    public static void AddLocalizeConfiguration(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            CultureInfo[] cultures =
            [
                new CultureInfo("en-US"),
                new CultureInfo("ka-GE"),
            ];

            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;

            options.DefaultRequestCulture = new RequestCulture("en-US");
        });
    }
}