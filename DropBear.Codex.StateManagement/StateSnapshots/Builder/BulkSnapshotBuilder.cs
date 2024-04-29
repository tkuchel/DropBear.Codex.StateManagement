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
        foreach (var builder in _builders.Select(entry => entry.Value))
            if (_registry is not null && ((dynamic)builder)._registryKey is not null)
                _registry.Register(((dynamic)builder).Build(), ((dynamic)builder)._registryKey);
            else
                ((dynamic)builder).Build(); // Build without registering, or handle as needed
    }
}
