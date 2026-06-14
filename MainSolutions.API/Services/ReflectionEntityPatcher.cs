using System.Reflection;
using MainSolutions.API.Services.Interfaces;

namespace MainSolutions.API.Services;

public sealed class ReflectionEntityPatcher : IEntityPatcher
{
    public void Apply<T>(T entity, IDictionary<string, object?> fields) where T : class
    {
        var entityType = typeof(T);

        foreach (var (key, value) in fields)
        {
            var property = entityType.GetProperty(
                key,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property is null || !property.CanWrite) continue;

            if (value is null)
            {
                property.SetValue(entity, null);
                continue;
            }

            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            try
            {
                var converted = targetType.IsEnum
                    ? Enum.Parse(targetType, value.ToString()!, ignoreCase: true)
                    : Convert.ChangeType(value.ToString(), targetType);

                property.SetValue(entity, converted);
            }
            catch
            {
                // skip fields that can't be converted
            }
        }
    }
}
