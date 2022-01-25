using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace CleanSolution.Infrastructure.Persistence.Extensions;
internal static class IQueryableExtensions
{
    // სრული დაფარვა, ახდენს ყველა იმ კლასთან Include() რომელსაც კი შეიცავს მოცემული კლასი
    internal static IQueryable<TEntity> Including<TEntity>(this IQueryable<TEntity> query) where TEntity : class
    {
        foreach (var item in query)
        {
            foreach (var property in item.GetType().GetProperties())
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    query = query.Include(property.Name);
            break;
        }

        return query;
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
