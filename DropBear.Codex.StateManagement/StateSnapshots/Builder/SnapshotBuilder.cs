using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;
using DropBear.Codex.StateManagement.StateSnapshots.Models;

namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class SnapshotBuilder<T> : ISnapshotBuilder where T : ICloneable<T>
{
    private bool _automaticSnapshotting = true;
    private IStateComparer<T> _comparer;
    private ISnapshotManagerRegistry? _registry;
    private string? _registryKey;
    private TimeSpan _retentionTime = TimeSpan.FromHours(24);
    private TimeSpan _snapshotInterval = TimeSpan.FromMinutes(1);

    public SnapshotBuilder() => _comparer = new DefaultStateComparer<T>(); // Default comparer

    // Explicit ISnapshotBuilder implementation for building the snapshot manager
    object ISnapshotBuilder.Build() => Build();

    // Expose registry key for the interface if needed
    string ISnapshotBuilder.RegistryKey => _registryKey ?? typeof(T).FullName!;

    public SnapshotBuilder<T> WithAutomaticSnapshotting(bool enabled)
    {
        var newBuilder = Clone();
        newBuilder._automaticSnapshotting = enabled;
        return newBuilder;
    }

    public SnapshotBuilder<T> WithSnapshotInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.FromSeconds(1))
            throw new ArgumentException("Snapshot interval must be at least one second.", nameof(interval));

        var newBuilder = Clone();
        newBuilder._snapshotInterval = interval;
        return newBuilder;
    }

    public SnapshotBuilder<T> WithRetentionTime(TimeSpan retentionTime)
    {
        if (retentionTime < TimeSpan.Zero)
            throw new ArgumentException("Retention time cannot be negative.", nameof(retentionTime));

        var newBuilder = Clone();
        newBuilder._retentionTime = retentionTime;
        return newBuilder;
    }

    public SnapshotBuilder<T> WithRegistry(ISnapshotManagerRegistry registry, string? registryKey)
    {
        Console.WriteLine($"Registering type {typeof(T).Name} with key {registryKey}");

        var newBuilder = Clone();
        newBuilder._registry = registry;
        newBuilder._registryKey = registryKey ?? typeof(T).FullName;
        return newBuilder;
    }

    public SnapshotBuilder<T> WithComparer(IStateComparer<T> comparer)
    {
        var newBuilder = Clone();
        newBuilder._comparer = comparer;
        return newBuilder;
    }

    public StateSnapshotManager<T> Build()
    {
        ValidateConfiguration();

        if (_registry is null)
            throw new InvalidOperationException("Snapshot registry must be set before building.");

        if (string.IsNullOrEmpty(_registryKey))
            throw new InvalidOperationException("Registry key must be set before building.");

        // Attempt to get or create the manager from the registry.
        var manager =
            _registry.GetOrCreateManager(_registryKey, _automaticSnapshotting, _snapshotInterval, _retentionTime,
                _comparer);
        if (manager is null || manager.IsSuccess is false)
            throw new InvalidOperationException($"Failed to obtain a snapshot manager for key {_registryKey}.");

        return manager.Value;
    }


    private void ValidateConfiguration()
    {
        if (_snapshotInterval > _retentionTime)
            throw new InvalidOperationException("Snapshot interval cannot exceed the retention time.");
    }

    private SnapshotBuilder<T> Clone() =>
        (SnapshotBuilder<T>)MemberwiseClone(); // Correctly copies the current state of the builder
}
