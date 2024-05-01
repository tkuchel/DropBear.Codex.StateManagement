using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace DropBear.Codex.StateManagement.DeepCloning;

public static class ReflectionOptimizer
{
    private static readonly ConcurrentDictionary<Type, Collection<FieldInfo>> FieldsCache = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public static Collection<FieldInfo> GetFields(Type type)
    {
        if (FieldsCache.TryGetValue(type, out var fields)) return fields;

        fields = new Collection<FieldInfo>();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            fields.Add(field);

        FieldsCache.TryAdd(type, fields);

        return fields;
    }

    public static PropertyInfo[] GetProperties(Type type)
    {
        if (PropertiesCache.TryGetValue(type, out var properties)) return properties;

        properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        PropertiesCache.TryAdd(type, properties);

        return properties;
    }
}
