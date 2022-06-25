using System.Reflection;

namespace Core.Domain.Extensions;
public static class ObjectMerger
{
    public static TDest ApplyTo<TDest, TSrc>(this TDest dest, TSrc src) where TDest : class where TSrc : class
    {
        var destProperties = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name).ToList();
        var srcProperties = typeof(TSrc).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name).ToList();
        var commonProperties = destProperties.Intersect(srcProperties).ToList();

        foreach (var prop in commonProperties)
        {
            PropertyInfo? destInfo = typeof(TDest).GetProperty(prop);
            PropertyInfo? srcInfo = typeof(TSrc).GetProperty(prop);

            if (destInfo?.PropertyType != srcInfo?.PropertyType)
                continue;

            if (destInfo?.PropertyType != typeof(string) && !destInfo.PropertyType.IsValueType)
                continue;


            if (destInfo.GetValue(dest) != srcInfo?.GetValue(src))
                destInfo.SetValue(dest, srcInfo?.GetValue(src));
        }

        return dest;
    }
}