using System.Collections.Concurrent;
using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Models;
using R3;
using Result = DropBear.Codex.Core.Result;

namespace DropBear.Codex.StateManagement.StateSnapshots;

public class StateSnapshotManager<T>
{
    private readonly bool _automaticSnapshotting;
    private readonly TimeSpan _retentionTime;
    private readonly TimeSpan _snapshotInterval;
    private readonly ConcurrentDictionary<int, Snapshot<T>> _snapshots;
    private T _currentState;
    private int _currentVersion;
    private IDisposable _subscription;

    public StateSnapshotManager(bool automaticSnapshotting, TimeSpan snapshotInterval, TimeSpan retentionTime)
    {
        _automaticSnapshotting = automaticSnapshotting;
        _snapshotInterval = snapshotInterval;
        _retentionTime = retentionTime;
        _snapshots = new ConcurrentDictionary<int, Snapshot<T>>();
        _currentVersion = 0;
        _currentState = default; // Initialize the current state
    }

    public Result SubscribeModelChanges(ObservableModel<T> model)
    {
        try
        {
            if (_automaticSnapshotting)
                _subscription = model.StateChanged
                    .Debounce(_snapshotInterval, TimeProvider.System)
                    .Subscribe(HandleModelChanged);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    private void HandleModelChanged(T state)
    {
        var createResult = CreateSnapshot(state);
        if (!createResult.IsSuccess)
            // Log the failure or handle it appropriately
            Console.WriteLine("Failed to create snapshot: " + createResult.Error);
    }

    public Result CreateSnapshot(T currentState)
    {
        try
        {
            var snapshot = new Snapshot<T>(currentState);
            _snapshots[Interlocked.Increment(ref _currentVersion)] = snapshot;
            _currentState = currentState;
            _ = FireAndForgetAsync(CleanupOldSnapshots);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public Result RevertToSnapshot(int version)
    {
        // Check if the snapshot exists
        if (!_snapshots.TryGetValue(version, out var snapshot))
            return Result.Failure($"Snapshot with version {version} not found.");

        // Revert to the snapshot state
        _currentState = snapshot.State;
        _currentVersion = version;
        NotifyStateReverted();
        return Result.Success();
    }

    private void NotifyStateReverted() =>
        // Implement notification logic here, potentially invoking an event or a callback
        Console.WriteLine("State reverted to version " + _currentVersion);

    private static async Task FireAndForgetAsync(Func<Result> actionFunc) =>
        await Task.Run(() =>
        {
            try
            {
                var result = actionFunc();
                if (!result.IsSuccess) Console.WriteLine("Async operation failed: " + result.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in FireAndForgetAsync: " + ex.Message);
            }
        }).ConfigureAwait(false);

    public Result<bool> CompareSnapshots(int version1, int version2)
    {
        try
        {
            if (_snapshots.TryGetValue(version1, out var snapshot1) &&
                _snapshots.TryGetValue(version2, out var snapshot2))
                return Result<bool>.Success(EqualityComparer<T>.Default.Equals(snapshot1.State, snapshot2.State));
            return Result<bool>.Failure("One or both snapshots not found.");
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(e.Message);
        }
    }

    private Result CleanupOldSnapshots()
    {
        try
        {
            var cutoff = DateTime.UtcNow - _retentionTime;
            var oldKeys = _snapshots.Where(kvp => kvp.Value.Timestamp < cutoff).Select(kvp => kvp.Key).ToList();
            foreach (var key in oldKeys) _snapshots.TryRemove(key, out _);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public Result<bool> IsDirty(T currentState)
    {
        try
        {
            // Compare the current state with the last snapshot
            return _snapshots.TryGetValue(_currentVersion, out var lastSnapshot)
                ? Result<bool>.Success(!EqualityComparer<T>.Default.Equals(lastSnapshot.State, currentState))
                : Result<bool>.Success(true); // Consider dirty if no snapshot to compare
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(e.Message);
        }
    }

    public T GetCurrentState() => _currentState;

    public void Dispose() => _subscription?.Dispose();
}
