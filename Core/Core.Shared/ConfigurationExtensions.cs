using Microsoft.Extensions.Configuration;

namespace Core.Shared;

public static class ConfigurationExtensions
{
    public static string GetString(this IConfiguration configuration, string key) =>
         configuration[key] ?? throw new ArgumentNullException($"'{key}' value, into configuration not exists");
}