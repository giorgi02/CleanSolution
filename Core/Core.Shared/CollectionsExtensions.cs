namespace Core.Shared;

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
}
