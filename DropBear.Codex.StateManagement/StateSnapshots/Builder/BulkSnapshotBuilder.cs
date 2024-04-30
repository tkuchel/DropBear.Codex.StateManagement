using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class BulkSnapshotBuilder
{
    private readonly Dictionary<Type, object> _builders = new();
    private ISnapshotManagerRegistry? _registry;
    public BulkSnapshotBuilder UseRegistry(ISnapshotManagerRegistry registry)
    {
        _registry = registry;
        return this;
    }
    public SnapshotBuilder<T> ConfigureFor<T>() where T : ICloneable<T>, new()
    {
        var builder = new SnapshotBuilder<T>();
        _builders[typeof(T)] = builder;
        return builder;
    }
    public void BuildAll()
    {
        foreach (var entry in _builders)
        {
            // Check if the entry's value implements ISnapshotBuilder
            if (entry.Value is ISnapshotBuilder builder)
            {
                var manager = builder.Build(); // Build the manager using the builder

                // Check for nullity of the registry and the registry key
                if (_registry is null || string.IsNullOrEmpty(builder.RegistryKey)) continue;
                
                // Use reflection to obtain the generic method with the correct type parameter
                var method = _registry.GetType().GetMethod(nameof(ISnapshotManagerRegistry.Register));
                if (method == null)
                {
                    throw new InvalidOperationException("The Register method is not found on the registry.");
                }

                // Make the method generic based on the type of the manager
                var genericMethod = method.MakeGenericMethod(manager.GetType());
                genericMethod.Invoke(_registry, new[] { manager, builder.RegistryKey });
            }
            else
            {
                // Optionally handle the case where a builder is not correctly implemented
                throw new InvalidOperationException($"Builder for type {entry.Key.Name} does not implement ISnapshotBuilder.");
            }
        }
    }
}
