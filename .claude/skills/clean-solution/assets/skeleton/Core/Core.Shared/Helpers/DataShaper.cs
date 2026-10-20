using Core.Shared.Exceptions;
using System.Dynamic;
using System.Reflection;

namespace Core.Shared.Helpers;

/// <summary>
/// Returns only the fields the client asked for (?fields=id,firstName).
/// </summary>
public static class DataShaper
{
    public static ExpandoObject? ShapeAs<TSource>(this TSource source, string fields)
    {
        ArgumentNullException.ThrowIfNull(source);

        TSource[] sources = [source];
        return sources.ShapeAs<TSource>(fields).FirstOrDefault();
    }

    public static IEnumerable<ExpandoObject> ShapeAs<TSource>(this IEnumerable<TSource> source, string fields)
    {
        ArgumentNullException.ThrowIfNull(source);

        const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

        var properties = string.IsNullOrWhiteSpace(fields)
            ? typeof(TSource).GetProperties(flags)
            : fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(name => typeof(TSource).GetProperty(name, flags)
                    ?? throw new ApiValidationException($"Property '{name}' does not exist.", "fields"))
                .ToArray();

        var result = new List<ExpandoObject>();
        foreach (var item in source)
        {
            var shaped = new ExpandoObject();
            foreach (var property in properties)
                shaped.TryAdd(property.Name, property.GetValue(item));

            result.Add(shaped);
        }

        return result;
    }
}
