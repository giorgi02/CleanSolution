using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace Core.Application.Commons;

public static class CollectionsExtensions
{
    /// <summary>
    /// foreach
    /// </summary>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
            action(item);

        return collection;
    }


    /// <summary>
    /// დალაგება კლიენტის მოთხოვნისამებრ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="orderby"> orderby=lastName,birthDate desc </param>
    /// <returns></returns>
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderby)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(orderby))
            return source;

        if (!source.Any())
            return source;

        var orderParams = orderby.Trim().Split(',');
        // კლასში არსებული თვისებების ამოღება, რათა შემდეგ შემოწმდეს ....
        var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var queryBuilder = new StringBuilder();

        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
                continue;

            var propertyFromQueryName = param.Trim().Split(" ")[0];
            // მიღებული, დალაგების პარამეტრების შემოწმება: არსებობაზე და სისწორეზე.
            var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

            if (objectProperty == null)
                continue;

            var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";

            queryBuilder.Append($"{objectProperty.Name} {sortingOrder}, ");
        }

        var orderQuery = queryBuilder.ToString().TrimEnd(',', ' ');

        return source.OrderBy(orderQuery);
    }
}
