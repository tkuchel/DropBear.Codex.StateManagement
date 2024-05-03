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

    public SnapshotBuilder<T> WithAutomaticSnapshotting(bool enabled)
    {
        _automaticSnapshotting = enabled;
        return this;
    }

    public SnapshotBuilder<T> WithSnapshotInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.FromSeconds(1))
            throw new ArgumentException("Snapshot interval must be at least one second.", nameof(interval));

        _snapshotInterval = interval;
        return this;
    }

    public SnapshotBuilder<T> WithRetentionTime(TimeSpan retentionTime)
    {
        if (retentionTime < TimeSpan.Zero)
            throw new ArgumentException("Retention time cannot be negative.", nameof(retentionTime));

        _retentionTime = retentionTime;
        return this;
    }

    public SnapshotBuilder<T> WithRegistry(ISnapshotManagerRegistry registry, string? registryKey)
    {
        _registry = registry;
        _registryKey = registryKey ?? typeof(T).FullName;
        return this;
    }

    public SnapshotBuilder<T> WithComparer(IStateComparer<T> comparer)
    {
        _comparer = comparer;
        return this;
    }

    public StateSnapshotManager<T> Build()
    {
        if (_registry is null)
            throw new InvalidOperationException("Snapshot registry must be set before building.");

        if (string.IsNullOrEmpty(_registryKey))
            throw new InvalidOperationException("Registry key must be set before building.");

        var getOrCreateResult = _registry.GetOrCreateManager(_registryKey, _automaticSnapshotting, _snapshotInterval, _retentionTime, _comparer);
        
        if (!getOrCreateResult.IsSuccess)
            throw new InvalidOperationException(getOrCreateResult.ErrorMessage);
        
        return getOrCreateResult.Value;
    }
    
    // Explicit ISnapshotBuilder implementation for building the snapshot manager
    object ISnapshotBuilder.Build() => Build();

    // Expose registry key for the interface if needed
    string ISnapshotBuilder.RegistryKey => _registryKey ?? typeof(T).FullName!;
}