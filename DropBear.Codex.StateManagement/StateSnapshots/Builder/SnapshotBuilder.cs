#region

using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;
using DropBear.Codex.StateManagement.StateSnapshots.Models;

#endregion

namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class SnapshotBuilder<T> : ISnapshotBuilder<T> where T : ICloneable<T>
{
    private bool _automaticSnapshotting = true;
    private IStateComparer<T>? _comparer;
    private ISnapshotManagerRegistry? _registry;
    private string? _registryKey;
    private TimeSpan _retentionTime = TimeSpan.FromHours(24);
    private TimeSpan _snapshotInterval = TimeSpan.FromMinutes(1);

    public ISnapshotBuilder<T> WithAutomaticSnapshotting(bool enabled)
    {
        _automaticSnapshotting = enabled;
        return this;
    }

    public ISnapshotBuilder<T> WithSnapshotInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.FromSeconds(1))
        {
            throw new ArgumentException("Snapshot interval must be at least one second.", nameof(interval));
        }

        _snapshotInterval = interval;
        return this;
    }

    public ISnapshotBuilder<T> WithRetentionTime(TimeSpan retentionTime)
    {
        if (retentionTime < TimeSpan.Zero)
        {
            throw new ArgumentException("Retention time cannot be negative.", nameof(retentionTime));
        }

        _retentionTime = retentionTime;
        return this;
    }

    public ISnapshotBuilder<T> WithRegistry(ISnapshotManagerRegistry registry, string? registryKey = null)
    {
        _registry = registry;
        _registryKey = registryKey;
        return this;
    }

    public ISnapshotBuilder<T> WithComparer(IStateComparer<T>? comparer)
    {
        _comparer = comparer;
        return this;
    }

    public Result<StateSnapshotManager<T>> Build()
    {
        if (_registry is null)
        {
            return Result<StateSnapshotManager<T>>.Failure("Snapshot registry must be set before building.");
        }

        var registryKey = _registryKey ?? typeof(T).FullName ??
            throw new InvalidOperationException("Registry key cannot be null or empty.");

        return _registry.GetOrCreateManager(registryKey, _automaticSnapshotting, _snapshotInterval, _retentionTime,
            _comparer ?? new DefaultStateComparer<T>());
    }

    Result<object> ISnapshotBuilder.Build()
    {
        return Build().Map(manager => (object)manager);
    }

    string? ISnapshotBuilder.RegistryKey => _registryKey;
}
