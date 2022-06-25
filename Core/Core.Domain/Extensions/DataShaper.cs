using System.Dynamic;
using System.Reflection;

namespace Core.Domain.Extensions;
public static class DataShaper
{
    /// <summary>
    /// მხოლოდ კლიენტის მოთხოვნილი ველების ჩვენება ერთეულ ობიექტში.
    /// სასურველია TSource ტიპიზაციის მითითებაც
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static ExpandoObject? ShapeAs<TSource>(this TSource source, string fields)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        TSource[] sources = new TSource[] { source };
        return sources.ShapeData<TSource>(fields).FirstOrDefault();
    }

    /// <summary>
    /// მხოლოდ კლიენტის მოთხოვნილი ველების ჩვენება კოლექციაში.
    /// სასურველია TSource ტიპიზაციის მითითებაც
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static IEnumerable<ExpandoObject> ShapeData<TSource>(this IEnumerable<TSource> source, string fields)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var expandoObjectList = new List<ExpandoObject>();

        var propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // all public properties should be in the ExpandoObject
            var propertyInfos = typeof(TSource).GetProperties(
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                // trim each field, as it might contain leading 
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var propertyName = field.Trim();

                // use reflection to get the property on the source object
                // we need to include public and instance, b/c specifying a binding 
                // flag overwrites the already-existing binding flags.
                var propertyInfo = typeof(TSource).GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                    throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");


                propertyInfoList.Add(propertyInfo);
            }
        }

        // run through the source objects
        foreach (TSource sourceObject in source)
        {
            // create an ExpandoObject that will hold the 
            // selected properties & values
            var dataShapedObject = new ExpandoObject();

            // Get the value of each property we have to return.  For that,
            // we run through the list
            foreach (var property in propertyInfoList)
            {
                // GetValue returns the value of the property on the source object
                var propertyValue = property.GetValue(sourceObject);

                // add the field to the ExpandoObject
                dataShapedObject.TryAdd(property.Name, propertyValue);
            }

            // add the ExpandoObject to the list
            expandoObjectList.Add(dataShapedObject);
        }

        // return the list
        return expandoObjectList;
    }
}
