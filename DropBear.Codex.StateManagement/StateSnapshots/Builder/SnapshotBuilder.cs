namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class SnapshotBuilder<T>
{
    private bool _automaticSnapshotting = true;
    private SnapshotManagerRegistry? _registry;
    private string? _registryKey;
    private TimeSpan _retentionTime = TimeSpan.FromHours(24);
    private TimeSpan _snapshotInterval = TimeSpan.FromMinutes(1);

    public SnapshotBuilder<T> SetAutomaticSnapshotting(bool enabled)
    {
        var newBuilder = Clone();
        newBuilder._automaticSnapshotting = enabled;
        return newBuilder;
    }

    public SnapshotBuilder<T> SetSnapshotInterval(TimeSpan interval)
    {
        if (interval < TimeSpan.FromSeconds(1))
            throw new ArgumentException("Snapshot interval must be at least one second.", nameof(interval));

        var newBuilder = Clone();
        newBuilder._snapshotInterval = interval;
        return newBuilder;
    }

    public SnapshotBuilder<T> SetRetentionTime(TimeSpan retentionTime)
    {
        if (retentionTime < TimeSpan.Zero)
            throw new ArgumentException("Retention time cannot be negative.", nameof(retentionTime));

        var newBuilder = Clone();
        newBuilder._retentionTime = retentionTime;
        return newBuilder;
    }

    public SnapshotBuilder<T> UseRegistry(SnapshotManagerRegistry registry, string registryKey)
    {
        var newBuilder = Clone();
        newBuilder._registry = registry;
        newBuilder._registryKey = registryKey;
        return newBuilder;
    }

    public StateSnapshotManager<T> Build()
    {
        ValidateConfiguration();

        if (_registry is not null && !string.IsNullOrEmpty(_registryKey))
            return _registry.GetOrCreateManager<T>(_registryKey, _automaticSnapshotting, _snapshotInterval,
                _retentionTime);

        return new StateSnapshotManager<T>(_automaticSnapshotting, _snapshotInterval, _retentionTime);
    }

    private void ValidateConfiguration()
    {
        if (_snapshotInterval > _retentionTime)
            throw new InvalidOperationException("Snapshot interval cannot exceed the retention time.");
    }

    private SnapshotBuilder<T> Clone() =>
        (SnapshotBuilder<T>)MemberwiseClone(); // Correctly copies the current state of the builder
}
