using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Text.Json;
using Blake2Fast;
using Cysharp.Text;
using DropBear.Codex.AppLogger.Builders;
using DropBear.Codex.StateManagement.Extensions;
using DropBear.Codex.StateManagement.Interfaces;
using Microsoft.Extensions.Logging;
using ILoggerFactory = DropBear.Codex.AppLogger.Interfaces.ILoggerFactory;

namespace DropBear.Codex.StateManagement.ModelState;

public class ModelStateSnapshot : IModelStateSnapshot
{
    private readonly ILogger<ModelStateSnapshot> _logger;
    private readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesCache = new();
    private readonly MemoryCache _snapshotCache = MemoryCache.Default;
  
    public ModelStateSnapshot() => _logger = LoggerFactory.CreateLogger<ModelStateSnapshot>();

    private static ILoggerFactory LoggerFactory => new LoggerConfigurationBuilder()
        .SetLogLevel(LogLevel.Information)
        .EnableConsoleOutput()
        .Build();


    /// <summary>
    ///     Initializes monitoring of a model by taking a snapshot of its state and storing it in the cache with a sliding
    ///     expiration policy.
    /// </summary>
    /// <typeparam name="T">The type of the model to monitor.</typeparam>
    /// <param name="model">The model to monitor.</param>
    /// <param name="expiration">The amount of time before the snapshot expires and is removed from the cache.</param>
    /// <param name="trackChanges">Should changes to the entity be tracked.</param>
    /// <param name="options">Optional custom JsonSerializerOptions for serialization settings.</param>
    public void InitializeSnapshot<T>(T model, TimeSpan expiration, bool trackChanges = false,
        JsonSerializerOptions? options = null)
    {
        try
        {
            var serializedModel = JsonSerializer.Serialize(model, options);
            var cacheKey = GenerateCacheKey(serializedModel, typeof(T));
            var dataToStore = trackChanges ? serializedModel : ComputeHash(serializedModel);

            var cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = expiration };
            _snapshotCache.Set(new CacheItem(cacheKey, dataToStore), cacheItemPolicy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ZString.Format("Error initializing snapshot: {0}", ex.Message));
        }
    }

    /// <summary>
    ///     Checks if the model has changed since its snapshot was taken.
    /// </summary>
    /// <typeparam name="T">The type of the model to check.</typeparam>
    /// <param name="model">The current state of the model.</param>
    /// <param name="changedProperties">An enumerable of string representing the changed properties.</param>
    /// <param name="options">Optional custom JsonSerializerOptions for serialization settings.</param>
    /// <returns>True if the model has changed; otherwise, false.</returns>
    public bool HasModelChanged<T>(T model, out IEnumerable<string> changedProperties,
        JsonSerializerOptions? options = null)
    {
        changedProperties = Enumerable.Empty<string>();
        try
        {
            var serializedModel = JsonSerializer.Serialize(model, options);
            var cacheKey = GenerateCacheKey(serializedModel, typeof(T));

            if (_snapshotCache.Get(cacheKey) is not string storedData)
                return true; // Assume changed if snapshot not found

            // If storedData is a hash, then it's not tracking changes
            if (storedData.Length == ComputeHash(serializedModel).Length)
                return !ComputeHash(serializedModel).Equals(storedData, StringComparison.Ordinal);

            // Change tracking is enabled, compare serialized states
            changedProperties = GetChangedProperties<T>(serializedModel, storedData);
            return changedProperties.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ZString.Format("Error checking model changes: {0}", ex.Message));
            return true; // Assume changed in case of error
        }
    }

    /// <summary>
    ///     Clears a model's snapshot from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model whose snapshot should be cleared.</param>
    /// <param name="options">Optional custom JsonSerializerOptions for serialization settings.</param>
    public void ClearSnapshot<T>(T model, JsonSerializerOptions? options = null)
    {
        try
        {
            var serializedModel = JsonSerializer.Serialize(model, options);
            var cacheKey = GenerateCacheKey(serializedModel, typeof(T));
            _snapshotCache.Remove(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ZString.Format("Error clearing snapshot: {0}", ex.Message));
        }
    }


    /// <summary>
    ///     Retrieves changed properties between two model states.
    /// </summary>
    /// <typeparam name="T">Type of the models being compared.</typeparam>
    /// <param name="current">The current state of the model.</param>
    /// <param name="original">The original state of the model.</param>
    /// <returns>A distinct list of property names that have changed.</returns>
    private IEnumerable<string> GetChangedProperties<T>(string current, string original)
    {
        var currentObj = JsonSerializer.Deserialize<T>(current);
        var originalObj = JsonSerializer.Deserialize<T>(original);

        return CheckForChanges(currentObj, originalObj, string.Empty).Distinct(StringComparer.Ordinal);
    }

    /// <summary>
    ///     Recursively checks for changes between the current and original model states.
    /// </summary>
    private IEnumerable<string> CheckForChanges(object? current, object? original, string basePath)
    {
        if (current is null || original is null)
            yield break;

        var type = current.GetType();
        if (type != original.GetType())
            yield break;

        if (type.IsSimpleType() || type.IsEnum)
        {
            if (!Equals(current, original))
                yield return basePath.Trim('.');
        }
        else if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            // Handle collections with complex objects
            var currentItems = ((IEnumerable)current).Cast<object>().ToList();
            var originalItems = ((IEnumerable)original).Cast<object>().ToList();

            if (currentItems.Count != originalItems.Count)
                yield return basePath;
            else
                for (var i = 0; i < currentItems.Count; i++)
                    foreach (var change in CheckForChanges(currentItems[i], originalItems[i], $"{basePath}[{i}]"))
                        yield return change;
        }
        else
        {
            // Retrieve properties from cache or get and cache them
            var properties =
                _propertiesCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            foreach (var property in properties)
            {
                var currentPropValue = property.GetValue(current);
                var originalPropValue = property.GetValue(original);
                foreach (var change in CheckForChanges(currentPropValue, originalPropValue,
                             $"{basePath}.{property.Name}"))
                    yield return change;
            }
        }
    }

    /// <summary>
    ///     Generates a unique cache key based on the model's serialized state and type.
    /// </summary>
    /// <param name="serializedModel">The serialized representation of the model.</param>
    /// <param name="modelType">The type of the model.</param>
    /// <returns>A unique cache key.</returns>
    private static string GenerateCacheKey(string serializedModel, Type modelType) =>
        $"{modelType.FullName}_{ComputeHash(serializedModel)}";

    /// <summary>
    ///     Computes a Blake2b hash for the given input string.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>A base64 encoded hash of the input.</returns>
    private static string ComputeHash(string input)
    {
        var hash = Blake2b.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}
