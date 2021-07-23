using System;
using System.Text.Json;

namespace CleanSolution.Core.Domain.Functions
{
    public static class Converters
    {
        public static object ConvertStringTo(this string value, Type type)
        {
            if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return Guid.Parse(value);
            }
            else if (type.IsEnum)
            {
                return JsonSerializer.Deserialize(value, type);
            }
            else if (type.IsArray)
            {
                return JsonSerializer.Deserialize(value, type);
            }
            else
            {
                Type t = Nullable.GetUnderlyingType(type) ?? type;
                return (value == null) ? null : Convert.ChangeType(value, t);
            }
        }
    }
}
