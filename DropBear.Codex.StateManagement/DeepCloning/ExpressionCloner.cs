#region

using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using ReferenceEqualityComparer = DropBear.Codex.StateManagement.DeepCloning.Comparers.ReferenceEqualityComparer;

#endregion

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

    internal static Func<T, Dictionary<object, object>, T> GetCloner<T>()
    {
        if (ClonerCache.TryGetValue(typeof(T), out var cachedCloner))
        {
            return (Func<T, Dictionary<object, object>, T>)cachedCloner;
        }

        var type = typeof(T);
        var parameter = Expression.Parameter(type, "input");
        var trackParameter = Expression.Parameter(typeof(Dictionary<object, object>), "track");
        var body = BuildCloneExpression(type, parameter, trackParameter);
        //Console.WriteLine(body);
        var lambda = Expression.Lambda<Func<T, Dictionary<object, object>, T>>(body, parameter, trackParameter);
        var compiled = lambda.Compile();
        ClonerCache.TryAdd(type, compiled);
        return compiled;
    }

    internal static Expression BuildCloneExpression(Type type, Expression source, Expression track)
    {
        if (IsImmutable(type))
        {
            return source; // Return the original object for immutable types
        }

        var properties = ReflectionOptimizer.GetProperties(type);
        var bindings = new List<MemberBinding>();

        foreach (var property in properties)
        {
            if (!property.CanWrite || !property.CanRead)
            {
                continue; // Skip properties that cannot be read or written
            }

            var propertyExpression = Expression.Property(source, property);
            var propertyType = property.PropertyType;

            Expression clonedPropertyExpression;

            if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
            {
                clonedPropertyExpression = CollectionCloner.CloneCollection(propertyExpression, propertyType, track);
            }
            else if (!IsImmutable(propertyType))
            {
                clonedPropertyExpression = BuildCloneExpression(propertyType, propertyExpression, track);
            }
            else
            {
                clonedPropertyExpression = propertyExpression;
            }

            bindings.Add(Expression.Bind(property, clonedPropertyExpression));
        }

        return Expression.MemberInit(Expression.New(type), bindings);
    }


    private static bool IsImmutable(Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
        {
            return true; // Basic immutability check for system types
        }

        // Extended check for immutable collections or special classes
        return type.Namespace?.StartsWith("System.Collections.Immutable", StringComparison.OrdinalIgnoreCase) is true ||
               // Check if all properties have private setters or no setters at all, indicating immutability
               type.GetProperties().All(prop => prop.SetMethod?.IsPublic != true);
    }
}
