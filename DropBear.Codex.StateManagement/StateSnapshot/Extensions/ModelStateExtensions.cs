namespace DropBear.Codex.StateManagement.StateSnapshots.Extensions;

public static class ModelStateExtensions
{
    /// <summary>
    ///     Determines if a type is simple (non-complex and non-collection) or an enumeration.
    /// </summary>
    public static bool IsSimpleType(this Type type) =>
        type.IsValueType || type.IsPrimitive ||
        new[]
        {
            typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(Guid)
        }.Contains(type) ||
        Convert.GetTypeCode(type) is not TypeCode.Object || type.IsEnum;

    // Helper to add or get properties from cache
#pragma warning disable CA1859
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
#pragma warning restore CA1859
        Func<TKey, TValue> valueFactory)
    {
        if (dictionary.TryGetValue(key, out var value)) return value;
        value = valueFactory(key);
        dictionary[key] = value;

        return value;
    }
}
