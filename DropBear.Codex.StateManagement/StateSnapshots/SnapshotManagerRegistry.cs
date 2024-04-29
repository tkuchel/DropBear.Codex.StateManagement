using System.Collections.Concurrent;
using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

namespace DropBear.Codex.StateManagement.StateSnapshots;

public class SnapshotManagerRegistry : ISnapshotManagerRegistry
{
    private readonly ConcurrentDictionary<string, object> _managers = new(StringComparer.OrdinalIgnoreCase);

    public StateSnapshotManager<T> GetOrCreateManager<T>(string key, bool automaticSnapshotting,
        TimeSpan snapshotInterval, TimeSpan retentionTime) where T : ICloneable<T>
    {
        // Avoiding closure by using a local function
        StateSnapshotManager<T> CreateManager()
        {
            return new StateSnapshotManager<T>(automaticSnapshotting, snapshotInterval, retentionTime);
        }

        return (StateSnapshotManager<T>)_managers.GetOrAdd(key, _ => CreateManager());
    }

    public void CreateSnapshot<T>(string key, T currentState) where T : ICloneable<T>
    {
        var manager = GetOrCreateManager<T>(key, true, TimeSpan.FromMinutes(5), TimeSpan.FromDays(1));
        manager.CreateSnapshot(currentState);
    }

    public Result RevertToSnapshot<T>(string key, int version) where T : ICloneable<T>
    {
        if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
            return typedManager.RevertToSnapshot(version);
        return Result.Failure("Snapshot manager not found.");
    }

    public Result<bool> IsDirty<T>(string key, T currentState) where T : ICloneable<T>
    {
        if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
            return typedManager.IsDirty(currentState);
        return Result<bool>.Failure("Snapshot manager not found.");
    }

    public void DisposeAll()
    {
        foreach (var manager in _managers.Values)
            if (manager is IDisposable disposable)
                disposable.Dispose();
        _managers.Clear();
    }

    public void Register<T>(StateSnapshotManager<T> manager, string key) where T : ICloneable<T>
    {
        if (!_managers.TryAdd(key, manager))
            throw new InvalidOperationException($"A manager with the key '{key}' already exists.");
    }
}
