using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Reflection;

namespace Core.Domain.Extensions;
public static class CommonFunctions
{
    /// <summary>
    /// ღრმა კლონირება
    /// </summary>
    public static T? DeepClone<T>(this T obj) where T : notnull, new()
    {
        var type = obj.GetType();
        var result = (T?)Activator.CreateInstance(type);

        do
            foreach (var field in type!.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                field.SetValue(result, field.GetValue(obj));
        while ((type = type.BaseType) != typeof(object));

        return result;
    }

    public static int CalculateAge(DateTime birthDate)
    {
        int age = DateTime.Now.Year - birthDate.Year;
        if (DateTime.Now < birthDate.AddYears(age))
            age--;

        return age;
    }
    public static int CalculateAge(DateTime birthDate, DateTime actionDate)
    {
        int age = actionDate.Year - birthDate.Year;
        if (actionDate < birthDate.AddYears(age))
            age--;

        return age;
    }

    /// <summary>
    /// string-ისგან ობიექტის ფორმირება
    /// </summary>
    public static object? ConvertFromString(this string value, Type? type)
    {
        if (type == null) return null;

        if (type.IsArray)
        {
            return JsonSerializer.Deserialize(value, type);
        }
        else  //if (TypeDescriptor.GetConverter(typeof(string)).CanConvertTo(type))
        {
            return TypeDescriptor.GetConverter(type).ConvertFromString(value);
        }
        //else
        //{
        //    throw new Exception($"მოცემული ტიპის: {type.FullName} კონვერტაცია შეუძლებელია, ჩაამატეთ მექანიკურად");
        //}
    }

    public static string ConvertToEng(this string geoText)
    {
        var engText = new StringBuilder();

        foreach (var item in geoText.ToCharArray())
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

                var x => x
            });
        }
        return engText.ToString();
    }
}