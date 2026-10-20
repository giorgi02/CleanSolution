using System.Text;

namespace Core.Shared.Helpers;

public static class CommonFunctions
{
    /// <summary>
    /// Reflection-based deep clone for simple POCOs (public settable properties, parameterless constructor).
    /// </summary>
    public static T? DeepClone<T>(this T obj) where T : notnull, new()
    {
        var type = obj.GetType();
        var clone = (T?)Activator.CreateInstance(type);

        foreach (var property in type.GetProperties())
            if (property.CanWrite)
            {
                object? value = property.GetValue(obj);
                if (value != null && value.GetType().IsClass && !value.GetType().FullName!.StartsWith("System."))
                    property.SetValue(clone, value.DeepClone());
                else
                    property.SetValue(clone, value);
            }

        return clone;
    }

    /// <summary>
    /// Transliterates Georgian text to Latin characters (e.g. for search keys or file names).
    /// </summary>
    public static string ConvertToEng(this string geoText)
    {
        var engText = new StringBuilder();

        foreach (var item in geoText)
        {
            engText.Append(item switch
            {
                'ა' => "a",
                'ბ' => "b",
                'გ' => "g",
                'დ' => "d",
                'ე' => "e",
                'ვ' => "v",
                'ზ' => "z",
                'თ' => "th",
                'ი' => "i",
                'კ' => "k",
                'ლ' => "l",
                'მ' => "m",
                'ნ' => "n",
                'ო' => "o",
                'პ' => "p",
                'ჟ' => "zh",
                'რ' => "r",
                'ს' => "s",
                'ტ' => "t",
                'უ' => "u",
                'ფ' => "ph",
                'ქ' => "q",
                'ღ' => "gh",
                'ყ' => "y",
                'ჩ' => "ch",
                'ც' => "ts",
                'შ' => "sh",
                'ძ' => "dz",
                'წ' => "w",
                'ჭ' => "ch",
                'ხ' => "kh",
                'ჯ' => "j",
                'ჰ' => "h",

                var x => x.ToString()
            });
        }
        return engText.ToString();
    }
}
