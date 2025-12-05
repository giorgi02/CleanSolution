using System.Text;

namespace Core.Shared;

public static class CommonFunctions
{
    /// <summary>
    /// ღრმა კლონირება
    /// </summary>
    public static T? DeepClone<T>(this T obj) where T : notnull, new()
    {
        var type = obj.GetType();
        var properties = type.GetProperties();
        var clone = (T?)Activator.CreateInstance(type);

        foreach (var property in properties)
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