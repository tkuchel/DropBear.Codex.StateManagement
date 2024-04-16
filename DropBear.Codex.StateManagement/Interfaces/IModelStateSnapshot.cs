using System.Text.Json;

namespace DropBear.Codex.StateManagement.Interfaces;

public interface IModelStateSnapshot
{
    /// <summary>
    /// Initializes monitoring of a model by taking a snapshot of its state and storing it in the cache with a sliding expiration policy.
    /// </summary>
    /// <typeparam name="T">The type of the model to monitor.</typeparam>
    /// <param name="model">The model to monitor.</param>
    /// <param name="expiration">The amount of time before the snapshot expires and is removed from the cache.</param>
    /// <param name="trackChanges">Should changes to the entity be tracked.</param>
    /// <param name="options">Optional custom JsonSerializerOptions for serialization settings.</param>
    void InitializeSnapshot<T>(T model, TimeSpan expiration, bool trackChanges = false, JsonSerializerOptions? options = null);

    /// <summary>
    /// Checks if the model has changed since its snapshot was taken.
    /// </summary>
    /// <typeparam name="T">The type of the model to check.</typeparam>
    /// <param name="model">The current state of the model.</param>
    /// <param name="changedProperties">An enumerable of string representing the changed properties.</param>
    /// <param name="options">Optional custom JsonSerializerOptions for serialization settings.</param>
    /// <returns>True if the model has changed; otherwise, false.</returns>
    bool HasModelChanged<T>(T model, out IEnumerable<string> changedProperties, JsonSerializerOptions? options = null);

    /// <summary>
    /// Clears a model's snapshot from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model whose snapshot should be cleared.</param>
    /// <param name="options">Optional custom JsonSerializerOptions for serialization settings.</param>
    void ClearSnapshot<T>(T model, JsonSerializerOptions? options = null);
}
