using System;
using System.ComponentModel;
using System.Text.Json;

namespace CleanSolution.Core.Domain.Functions
{
    public static class Converters
    {
        // todo: დავხვეწო ეს მეთოდი
        public static object ConvertFromString(this string value, Type type)
        {
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
    }
}
