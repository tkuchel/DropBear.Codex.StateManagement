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
        return (StateSnapshotManager<T>)_managers.GetOrAdd(key, _ => CreateManager());

        // Avoiding closure by using a local function
        StateSnapshotManager<T> CreateManager()
        {
            return new StateSnapshotManager<T>(automaticSnapshotting, snapshotInterval, retentionTime);
        }
    }

    public void CreateSnapshot<T>(string key, T currentState) where T : ICloneable<T>
    {
        var manager = GetOrCreateManager<T>(key, true, TimeSpan.FromMinutes(5), TimeSpan.FromDays(1));
        manager.CreateSnapshot(currentState);
    }

    public async Task<Result> CreateSnapshotAsync<T>(string key, Task<T> currentStateTask) where T : ICloneable<T>
    {
        var manager = GetOrCreateManager<T>(key, automaticSnapshotting: true, TimeSpan.FromMinutes(5), TimeSpan.FromDays(1));
        return await manager.CreateSnapshotAsync(currentStateTask).ConfigureAwait(false);
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

    public async Task<Result<bool>> IsDirtyAsync<T>(string key, Task<T> currentStateTask) where T : ICloneable<T>
    {
        if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
            return await typedManager.IsDirtyAsync(currentStateTask).ConfigureAwait(false);
        return Result<bool>.Failure("Snapshot manager not found.");
    }


    public void DisposeAll()
    {
        foreach (var manager in _managers.Values.OfType<IDisposable>())
            manager.Dispose();

        _managers.Clear();
    }

    public Result<TManager> GetManager<TManager>(string key) where TManager : ICloneable<TManager>
    {
        if (_managers.TryGetValue(key, out var manager) && manager is TManager typedManager)
            return Result<TManager>.Success(typedManager);
        return Result<TManager>.Failure($"Snapshot manager for key '{key}' not found or wrong type.");
    }

    public void Register<T>(StateSnapshotManager<T> manager, string key, bool overwrite = false) where T : ICloneable<T>
    {
        switch (overwrite)
        {
            case false when !_managers.TryAdd(key, manager):
                Console.WriteLine($"Snapshot manager with key '{key}' already exists.");
                return;
            case true when _managers.TryUpdate(key, manager, _managers[key]):
                Console.WriteLine($"Snapshot manager with key '{key}' updated.");
                break;
            case true:
                Console.WriteLine($"Failed to update snapshot manager with key '{key}'.");
                break;
        }
    }
}
