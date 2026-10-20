using System.Reflection;
using System.Text;

namespace Core.Shared.Helpers;

public static class HttpQueryStrings
{
    /// <summary>
    /// Converts an object into a query string for HTTP GET calls: new { Id = 1, Tags = ["a", "b"] } => ?Id=1&amp;Tags=a&amp;Tags=b.
    /// Default-valued value types are skipped; nested classes are flattened as Parent.Child=value.
    /// </summary>
    public static string ToQueryString<T>(this T @this) where T : class
    {
        var query = new StringBuilder();

        BuildQueryString(@this, query);

        if (query.Length > 0) query[0] = '?';

        return query.ToString();
    }

    private static void BuildQueryString(object? obj, StringBuilder query, string prefix = "")
    {
        if (obj == null) return;

        foreach (var p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = p.GetValue(obj);
            if (value == null) continue;

            if (value is DateTime[] dates)
                foreach (var item in dates)
                    query.Append($"&{prefix}{p.Name}={item:yyyy-MM-dd}");

            else if (value is Array array)
                foreach (var item in array)
                    query.Append($"&{prefix}{p.Name}={Uri.EscapeDataString(Convert.ToString(item) ?? string.Empty)}");

            else if (value is string text)
                query.Append($"&{prefix}{p.Name}={Uri.EscapeDataString(text)}");

            else if (value is DateTime date && date != default)
                query.Append($"&{prefix}{p.Name}={date:yyyy-MM-dd}");

            else if (p.PropertyType.IsValueType && !value.Equals(Activator.CreateInstance(p.PropertyType)))
                query.Append($"&{prefix}{p.Name}={value}");

            else if (p.PropertyType.IsClass)
                BuildQueryString(value, query, $"{prefix}{p.Name}.");
        }
    }
}
