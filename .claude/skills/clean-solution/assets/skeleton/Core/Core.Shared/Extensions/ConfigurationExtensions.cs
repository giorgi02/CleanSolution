using Microsoft.Extensions.Configuration;

namespace Core.Shared.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Reads a required configuration value and fails fast when the key is missing.
    /// </summary>
    public static string GetString(this IConfiguration configuration, string key) =>
        configuration[key] ?? throw new InvalidOperationException($"Configuration value '{key}' is missing.");
}
