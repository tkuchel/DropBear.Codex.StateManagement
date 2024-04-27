namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class SnapshotBuilder<T>
{
    private bool _automaticSnapshotting = true;
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

    public StateSnapshotManager<T> Build()
    {
        ValidateConfiguration();
        return new StateSnapshotManager<T>(_automaticSnapshotting, _snapshotInterval, _retentionTime);
    }

    private void ValidateConfiguration()
    {
        // Add any complex validation logic that needs multiple properties to be considered here
    }

    private SnapshotBuilder<T> Clone() =>
        // Create a copy of the current builder to maintain immutability
        (SnapshotBuilder<T>)MemberwiseClone();
}
