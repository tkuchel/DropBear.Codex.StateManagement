using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.DeepCloning.Attributes;
using Newtonsoft.Json;
using ReferenceEqualityComparer = DropBear.Codex.StateManagement.DeepCloning.Comparers.ReferenceEqualityComparer;

namespace DropBear.Codex.StateManagement.DeepCloning;

public static class DeepCloner
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects
    };

    // Caching compiled cloning functions for expression-based cloning
    private static readonly ConcurrentDictionary<Type, Delegate> ClonerCache = new();

    public static Result<T> Clone<T>(T source, JsonSerializerSettings? settings = null) where T : class
    {
        if (UseExpressionBasedCloning(typeof(T)))
            try
            {
                var cloner = ExpressionCloner.GetCloner<T>();
                var track = new Dictionary<object, object>(new ReferenceEqualityComparer());
                var clonedObject = cloner(source, track);
                return Result<T>.Success(clonedObject);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cloning object with expression-based cloner: {ex.Message}");
                return Result<T>.Failure("An error occurred while cloning the object: " + ex.Message);
            }

        // Fallback to JSON-based cloning
        try
        {
            var jsonSettings = settings ?? DefaultSettings;
            var json = JsonConvert.SerializeObject(source, jsonSettings);
            var clonedObject = JsonConvert.DeserializeObject<T>(json, jsonSettings);
            if (clonedObject is null)
                throw new InvalidOperationException("Cloning resulted in a null object.");
            return Result<T>.Success(clonedObject);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cloning object with JSON: {ex.Message}");
            return Result<T>.Failure("An error occurred while cloning the object: " + ex.Message);
        }
    }

    private static bool UseExpressionBasedCloning(Type type)
    {
        // Check if there's a CloneMethodAttribute and return its value
        var attribute = type.GetCustomAttribute<CloneMethodAttribute>();
        if (attribute is not null) return attribute.UseExpression;

        // Check for immutability or simple types
        if (IsImmutable(type)) return true;

        // Additional logic based on type complexity or size
        // For example, prefer JSON cloning for types known to serialize well
        return type.GetProperties().Length <= 10 && type.GetFields().Length <= 10; // Arbitrary complexity measure
    }

    private static bool IsImmutable(Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
            return true;

        // Further immutability checks can be added here
        return type.GetProperties().All(prop => prop.GetSetMethod() == null);
    }

    // private static Func<T, Dictionary<object, object>, T> GetExpressionCloner<T>()
    // {
    //     if (ClonerCache.TryGetValue(typeof(T), out var cachedCloner))
    //         return (Func<T, Dictionary<object, object>, T>)cachedCloner;
    //     var parameter = Expression.Parameter(typeof(T), "input");
    //     var trackParameter = Expression.Parameter(typeof(Dictionary<object, object>), "track");
    //     var body = ExpressionCloner.BuildCloneExpression(typeof(T), parameter, trackParameter);
    //     var lambda = Expression.Lambda<Func<T, Dictionary<object, object>, T>>(body, parameter, trackParameter);
    //     var compiled = lambda.Compile();
    //     ClonerCache.TryAdd(typeof(T), compiled);
    //     return compiled;
    // }
}
