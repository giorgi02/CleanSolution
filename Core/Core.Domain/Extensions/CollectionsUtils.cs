using Core.Domain.Basics;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace Core.Domain.Extensions;
public static class CollectionsUtils
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

    public static TSource? FindSingle<TSource>(this IQueryable<TSource> source, Guid id) where TSource : BaseEntity
    {
        if (source == null) throw new NullReferenceException(nameof(source));
        foreach (TSource element in source)
            if (element.Id == id) return element;

        return default;
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
        if (source == null)
            throw new ArgumentNullException(nameof(source));

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
