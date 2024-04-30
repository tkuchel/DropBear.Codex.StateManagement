using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using DropBear.Codex.StateManagement.DeepCloning.Attributes;
using ReferenceEqualityComparer = DropBear.Codex.StateManagement.DeepCloning.Comparers.ReferenceEqualityComparer;

namespace DropBear.Codex.StateManagement.DeepCloning;

public static class ExpressionCloner
{
    private static readonly ConcurrentDictionary<Type, Delegate> ClonerCache = new();

    public static T Clone<T>(T original)
    {
        var cloner = GetCloner<T>();
        var track = new Dictionary<object, object>(new ReferenceEqualityComparer());
        return cloner(original, track);
    }

    private static Func<T, Dictionary<object, object>, T> GetCloner<T>()
    {
        if (ClonerCache.TryGetValue(typeof(T), out var cachedCloner))
            return (Func<T, Dictionary<object, object>, T>)cachedCloner;
        var type = typeof(T);
        var parameter = Expression.Parameter(type, "input");
        var trackParameter = Expression.Parameter(typeof(Dictionary<object, object>), "track");
        var body = BuildCloneExpression(type, parameter, trackParameter);
        var lambda = Expression.Lambda<Func<T, Dictionary<object, object>, T>>(body, parameter, trackParameter);
        var compiled = lambda.Compile();
        ClonerCache.TryAdd(type, compiled);
        return compiled;
    }

internal static Expression BuildCloneExpression(Type type, Expression source, Expression track)
{
    if (IsImmutable(type))
        return source; // Return the original object for immutable types, includes value types and strings

    var fields = ReflectionOptimizer.GetFields(type);
    
    // Attempt to retrieve the MemberwiseClone method with the appropriate binding flags
    var cloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
    if (cloneMethod == null)
        throw new InvalidOperationException("Could not find the MemberwiseClone method necessary for cloning.");

    // Proceed with creating the cloning expression using the retrieved method
    var cloneExpression = Expression.Call(source, cloneMethod);

    // Cast the cloneExpression back to the original type
    var typedCloneExpression = Expression.Convert(cloneExpression, type);

    var bindings = new List<MemberBinding>();
    foreach (var field in fields)
    {
        var cloneableAttr = field.GetCustomAttribute<CloneableAttribute>();
        if (cloneableAttr is not null && !cloneableAttr.IsCloneable)
            continue; // Skip cloning this field if it's marked as not cloneable

        var fieldType = field.FieldType;
        var fieldExpression = Expression.Field(typedCloneExpression, field);

        // Check if the field itself is a collection and handle accordingly
        if (typeof(IEnumerable).IsAssignableFrom(fieldType) && fieldType != typeof(string))
        {
            var clonedField = CollectionCloner.CloneCollection(fieldExpression, fieldType);
            bindings.Add(Expression.Bind(field, clonedField));
        }
        else if (!IsImmutable(fieldType)) // Continue to use IsImmutable to check for types that need deep cloning
        {
            var clonedField = BuildCloneExpression(fieldType, fieldExpression, track);
            bindings.Add(Expression.Bind(field, clonedField));
        }
        else
        {
            // Directly bind immutable or value type or string fields
            bindings.Add(Expression.Bind(field, fieldExpression));
        }
    }

    return Expression.MemberInit(Expression.New(type), bindings);
}



    private static bool IsImmutable(Type type)
    {
        if (type.IsValueType || type == typeof(string))
            return true; // Extend this to include other system types or custom immutable types

        // Check if all fields are readonly
        return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .All(f => f.IsInitOnly);
    }
}
