namespace Core.Shared.Extensions;

public static class CollectionsExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
            action(item);

        return collection;
    }
}
