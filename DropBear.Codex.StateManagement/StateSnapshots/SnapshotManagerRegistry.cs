#region

using System.Collections.Concurrent;
using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

#endregion

namespace DropBear.Codex.StateManagement.StateSnapshots;

public class SnapshotManagerRegistry : ISnapshotManagerRegistry
{
    private readonly ConcurrentDictionary<string, object> _managers = new(StringComparer.OrdinalIgnoreCase);

    public Result<StateSnapshotManager<T>> GetOrCreateManager<T>(string key, bool automaticSnapshotting,
        TimeSpan snapshotInterval, TimeSpan retentionTime, IStateComparer<T>? comparer = null) where T : ICloneable<T>
    {
        try
        {
            return Result<StateSnapshotManager<T>>.Success(
                _managers.GetOrAdd(key, _ => CreateManager()) as StateSnapshotManager<T> ??
                throw new InvalidOperationException(
                    $"Snapshot manager for key '{key}' is not of type StateSnapshotManager<{typeof(T).Name}>"));

            // Local function to create a new manager
            StateSnapshotManager<T> CreateManager()
            {
                return new StateSnapshotManager<T>(automaticSnapshotting, snapshotInterval, retentionTime, comparer);
            }
        }
        catch (Exception ex)
        {
            return Result<StateSnapshotManager<T>>.Failure($"Failed to create snapshot manager: {ex.Message}", ex);
        }
    }

    public Result CreateSnapshot<T>(string key, T currentState) where T : ICloneable<T>
    {
        return ExecuteManagerAction<T>(key, manager => manager.CreateSnapshot(currentState));
    }

    public Task<Result> CreateSnapshotAsync<T>(string key, Task<T> currentStateTask) where T : ICloneable<T>
    {
        return ExecuteManagerActionAsync<T>(key, manager => manager.CreateSnapshotAsync(currentStateTask));
    }

    public Result RevertToSnapshot<T>(string key, int version) where T : ICloneable<T>
    {
        return ExecuteManagerAction<T>(key, manager => manager.RevertToSnapshot(version));
    }

    public Result<bool> IsDirty<T>(string key, T currentState) where T : ICloneable<T>
    {
        return ExecuteManagerFunc<T, bool>(key, manager => manager.IsDirty(currentState));
    }

    public Task<Result<bool>> IsDirtyAsync<T>(string key, Task<T> currentStateTask) where T : ICloneable<T>
    {
        return ExecuteManagerFuncAsync<T, bool>(key, manager => manager.IsDirtyAsync(currentStateTask));
    }

    public void DisposeAll()
    {
        foreach (var disposableManager in _managers.Values.OfType<IDisposable>())
        {
            disposableManager.Dispose();
        }

        _managers.Clear();
    }

    public Result<StateSnapshotManager<T>> GetManager<T>(string key) where T : ICloneable<T>
    {
        if (_managers.TryGetValue(key, out var manager) && manager is StateSnapshotManager<T> typedManager)
        {
            return Result<StateSnapshotManager<T>>.Success(typedManager);
        }

        return Result<StateSnapshotManager<T>>.Failure($"Snapshot manager for key '{key}' not found or wrong type.");
    }

    public Result Register<T>(StateSnapshotManager<T> manager, string key, bool overwrite = false)
        where T : ICloneable<T>
    {
        var added = overwrite
            ? _managers.TryUpdate(key, manager, _managers.GetValueOrDefault(key)!)
            : _managers.TryAdd(key, manager);

        return added
            ? Result.Success()
            : Result.Failure($"Failed to register snapshot manager with key '{key}'.");
    }

    public Result<int> GetCurrentVersion<T>(string key) where T : ICloneable<T>
    {
        return ExecuteManagerFunc<T, int>(key, manager => manager.GetCurrentVersion());
    }

    public Result<T?> GetCurrentState<T>(string key) where T : ICloneable<T>
    {
        return ExecuteManagerFunc<T, T?>(key, manager => Result<T?>.Success(manager.GetCurrentState()));
    }

    private Result ExecuteManagerAction<T>(string key, Func<StateSnapshotManager<T>, Result> action)
        where T : ICloneable<T>
    {
        return GetManager<T>(key)
            .OnSuccess(action)
            .OnFailure((error, _) => Result.Failure(error))
            .Unwrap();
    }

    private Result<TResult> ExecuteManagerFunc<T, TResult>(string key,
        Func<StateSnapshotManager<T>, Result<TResult>> func) where T : ICloneable<T>
    {
        return GetManager<T>(key)
            .OnSuccess(func)
            .OnFailure((error, _) => Result<TResult>.Failure(error))
            .Unwrap();
    }

    private async Task<Result> ExecuteManagerActionAsync<T>(string key,
        Func<StateSnapshotManager<T>, Task<Result>> action) where T : ICloneable<T>
    {
        var managerResult = GetManager<T>(key);
        return managerResult.IsSuccess
            ? await action(managerResult.Value).ConfigureAwait(false)
            : Result.Failure(managerResult.ErrorMessage ?? "An unknown error has occurred.");
    }

    private async Task<Result<TResult>> ExecuteManagerFuncAsync<T, TResult>(string key,
        Func<StateSnapshotManager<T>, Task<Result<TResult>>> func) where T : ICloneable<T>
    {
        var managerResult = GetManager<T>(key);
        return managerResult.IsSuccess
            ? await func(managerResult.Value).ConfigureAwait(false)
            : Result<TResult>.Failure(managerResult.ErrorMessage ?? "An unknown error has occurred.");
    }
}
