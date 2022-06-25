using System.Reflection;
using System.Text;

namespace Core.Domain.Extensions;
public static class HttpQueryStrings
{
    private static readonly StringBuilder _query = new();
    // ქვედა 2 მეთოდი ახდენენ კლასის მოთხოვნად გადაქცევას HttpGet მეთოდისთვის (object to query)
    public static string ToQueryString<T>(this T @this) where T : class
    {
        _query.Clear();

        BuildQueryString(@this, "");

        if (_query.Length > 0) _query[0] = '?';

        return _query.ToString();
    }

    private static void BuildQueryString<T>(T? obj, string prefix = "") where T : class
    {
        if (obj == null) return;

        foreach (var p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetValue(obj, Array.Empty<object>()) != null)
            {
                var value = p.GetValue(obj, Array.Empty<object>());


                // DateTime[]
                if (p.PropertyType.IsArray && value?.GetType() == typeof(DateTime[]))
                    foreach (var item in (DateTime[])value)
                        _query.Append($"&{prefix}{p.Name}={item.ToString("yyyy-MM-dd")}");

                // მასივებისთვის
                else if (p.PropertyType.IsArray)
                    foreach (var item in (Array)value!)
                        _query.Append($"&{prefix}{p.Name}={item}");

                else if (p.PropertyType == typeof(string))
                    _query.Append($"&{prefix}{p.Name}={value}");

                else if (p.PropertyType == typeof(DateTime) && !value!.Equals(Activator.CreateInstance(p.PropertyType))) // is not default 
                    _query.Append($"&{prefix}{p.Name}={((DateTime)value).ToString("yyyy-MM-dd")}");

                else if (p.PropertyType.IsValueType && !value!.Equals(Activator.CreateInstance(p.PropertyType))) // is not default 
                    _query.Append($"&{prefix}{p.Name}={value}");


                else if (p.PropertyType.IsClass)
                    BuildQueryString(value, $"{prefix}{p.Name}.");
            }
        }
    }
}
