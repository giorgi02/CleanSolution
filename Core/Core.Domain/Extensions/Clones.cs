using System.Reflection;

namespace Core.Domain.Extensions
{
    public static class Clones
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
    }
}
