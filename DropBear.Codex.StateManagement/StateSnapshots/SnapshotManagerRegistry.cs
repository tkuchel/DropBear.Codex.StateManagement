using System.Collections.Concurrent;
using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

namespace DropBear.Codex.StateManagement.StateSnapshots;

public class SnapshotManagerRegistry : ISnapshotManagerRegistry
{
    private readonly ConcurrentDictionary<string, object> _managers = new(StringComparer.OrdinalIgnoreCase);

    public Result<StateSnapshotManager<T>> GetOrCreateManager<T>(string key, bool automaticSnapshotting,
        TimeSpan snapshotInterval, TimeSpan retentionTime) where T : ICloneable<T>
    {
        try
        {
            if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
                return Result<StateSnapshotManager<T>>.Success(typedManager);


            // Avoiding closure by using a local function
            StateSnapshotManager<T> CreateManager()
            {
                var newManager = new StateSnapshotManager<T>(automaticSnapshotting, snapshotInterval, retentionTime);
                _managers.TryAdd(key, newManager);
                return newManager;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<StateSnapshotManager<T>>.Failure("Failed to create snapshot manager.");
        }

        return Result<StateSnapshotManager<T>>.Failure("Failed to create snapshot manager.");
    }

    public Result CreateSnapshot<T>(string key, T currentState) where T : ICloneable<T>
    {
        try
        {
            if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
                return typedManager.CreateSnapshot(currentState);
            return Result.Failure("Snapshot manager not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result.Failure("Failed to create snapshot.");
        }
    }

    public async Task<Result> CreateSnapshotAsync<T>(string key, Task<T> currentStateTask) where T : ICloneable<T>
    {
        try
        {
            if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
                return await typedManager.CreateSnapshotAsync(currentStateTask).ConfigureAwait(false);
            return Result.Failure("Snapshot manager not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result.Failure("Failed to create snapshot.");
        }
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

    public Result<StateSnapshotManager<T>> GetManager<T>(string key) where T : ICloneable<T>
    {
        if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
            return Result<StateSnapshotManager<T>>.Success(typedManager);
        return Result<StateSnapshotManager<T>>.Failure($"Snapshot manager for key '{key}' not found or wrong type.");
    }

    public Result Register<T>(StateSnapshotManager<T> manager, string key, bool overwrite = false)
        where T : ICloneable<T>
    {
        switch (overwrite)
        {
            case false when !_managers.TryAdd(key, manager):
                Console.WriteLine($"Snapshot manager with key '{key}' already exists.");
                return Result.PartialSuccess($"Snapshot manager with key '{key}' already exists.");
            case true when _managers.TryUpdate(key, manager, _managers[key]):
                Console.WriteLine($"Snapshot manager with key '{key}' updated.");
                return Result.Success();
            case true:
                Console.WriteLine($"Failed to update snapshot manager with key '{key}'.");
                return Result.Failure($"Failed to update snapshot manager with key '{key}'.");
        }

        Console.WriteLine($"Failed to register snapshot manager with key '{key}'.");
        return Result.Failure($"Failed to register snapshot manager with key '{key}'.");
    }
}
