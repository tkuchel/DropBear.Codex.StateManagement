using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;

namespace DropBear.Codex.StateManagement.DeepCloning;

public static class ReflectionOptimizer
{
    private static readonly ConcurrentDictionary<Type, Collection<FieldInfo>> FieldsCache = new();

    public static Collection<FieldInfo> GetFields(Type type)
    {
        if (FieldsCache.TryGetValue(type, out var fields)) return fields;

        fields = new Collection<FieldInfo>();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            fields.Add(field);

        FieldsCache.TryAdd(type, fields);

        return fields;
    }
}
