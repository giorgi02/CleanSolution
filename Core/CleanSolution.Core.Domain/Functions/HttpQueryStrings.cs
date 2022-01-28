using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CleanSolution.Core.Domain.Functions;
public static class HttpQueryStrings
{
    // ქვედა 2 მეთოდი ახდენენ კლასის მოთხოვნად გადაქცევას HttpGet მეთოდისთვის (object to query)
    public static string ToQueryString<T>(this T @this) where T : class
    {
        StringBuilder query = @this.ToQueryString("");

        if (query.Length > 0)
            query[0] = '?';

        return query.ToString();
    }

    private static StringBuilder ToQueryString<T>(this T obj, string prefix = "") where T : class
    {
        StringBuilder gatherer = new StringBuilder();

        foreach (var p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetValue(obj, new object[0]) != null)
            {
                var value = p.GetValue(obj, new object[0]);

                // DateTime[]
                if (p.PropertyType.IsArray && value?.GetType() == typeof(DateTime[]))
                    foreach (var item in (DateTime[])value)
                        gatherer.Append($"&{prefix}{p.Name}={item.ToString("yyyy-MM-dd")}");

                // მასივებისთვის
                else if (p.PropertyType.IsArray)
                    foreach (var item in (Array)value!)
                        gatherer.Append($"&{prefix}{p.Name}={item}");

                else if (p.PropertyType == typeof(string))
                    gatherer.Append($"&{prefix}{p.Name}={value}");

                else if (p.PropertyType == typeof(DateTime) && !value!.Equals(Activator.CreateInstance(p.PropertyType))) // is not default 
                    gatherer.Append($"&{prefix}{p.Name}={((DateTime)value).ToString("yyyy-MM-dd")}");

                else if (p.PropertyType.IsValueType && !value!.Equals(Activator.CreateInstance(p.PropertyType))) // is not default 
                    gatherer.Append($"&{prefix}{p.Name}={value}");


                else if (p.PropertyType.IsClass)
                    gatherer.Append(value?.ToQueryString($"{prefix}{p.Name}."));
            }
        }

        return gatherer;
    }

    #region HttpClient Extensions
    public static async Task<T?> ReadContentAs<T>(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");

        var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonSerializer.Deserialize<T>(dataAsString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    #endregion
}
