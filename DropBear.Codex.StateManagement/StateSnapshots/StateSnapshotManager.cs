using System.Collections.Concurrent;
using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;
using DropBear.Codex.StateManagement.StateSnapshots.Models;
using R3;
using Result = DropBear.Codex.Core.Result;

namespace DropBear.Codex.StateManagement.StateSnapshots;

public class StateSnapshotManager<T> : IDisposable where T : ICloneable<T>
{
    private readonly bool _automaticSnapshotting;
    private readonly IStateComparer<T> _comparer;
    private readonly TimeSpan _retentionTime;
    private readonly TimeSpan _snapshotInterval;
    private readonly ConcurrentDictionary<int, Snapshot<T>> _snapshots;
    private readonly Subject<T> _stateRevertedSubject = new();
    private T? _currentState;
    private int _currentVersion;
    private IDisposable? _subscription;

    public StateSnapshotManager(bool automaticSnapshotting, TimeSpan snapshotInterval, TimeSpan retentionTime,
        IStateComparer<T>? comparer = null)
    {
        _automaticSnapshotting = automaticSnapshotting;
        _snapshotInterval = snapshotInterval;
        _retentionTime = retentionTime;
        _snapshots = new ConcurrentDictionary<int, Snapshot<T>>();
        _currentVersion = 0;
        _currentState = default!;
        _comparer = comparer ?? new DefaultStateComparer<T>();
    }

    public Observable<T> StateReverted => _stateRevertedSubject.AsObservable();

    public void Dispose()
    {
        _stateRevertedSubject.Dispose();
        DisposeCurrentSubscription();
    }

    public Result SubscribeModelChanges(Observable<T> modelStateChanges)
    {
        if (!_automaticSnapshotting)
            return Result.Success(); // Return success if not subscribing due to manual control

        DisposeCurrentSubscription(); // Ensure any existing subscription is disposed

        try
        {
            _subscription = modelStateChanges
                .Debounce(_snapshotInterval)
                .Subscribe(HandleModelChanged);

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    private void DisposeCurrentSubscription()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    private void HandleModelChanged(T state) => CreateSnapshot(state);

    public Result CreateSnapshot(T currentState)
    {
        try
        {
            var snapshot = new Snapshot<T>(currentState.Clone());
            _snapshots[Interlocked.Increment(ref _currentVersion)] = snapshot;
            _currentState = currentState;
            _ = CleanupOldSnapshotsAsync(); // Fire and forget cleanup task
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> CreateSnapshotAsync(Task<T> currentStateTask)
    {
        try
        {
            var currentState = await currentStateTask.ConfigureAwait(false);
            return CreateSnapshot(currentState);
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public Result RevertToSnapshot(int version)
    {
        if (!_snapshots.TryGetValue(version, out var snapshot))
            return Result.Failure("Snapshot not found.");

        _currentState = snapshot.State.Clone();
        _currentVersion = version;
        NotifyStateReverted(_currentState);
        return Result.Success();
    }

    private void NotifyStateReverted(T state) => _stateRevertedSubject.OnNext(state);

    public Result<bool> CompareSnapshots(int version1, int version2)
    {
        if (!_snapshots.TryGetValue(version1, out var snapshot1) ||
            !_snapshots.TryGetValue(version2, out var snapshot2))
            return Result<bool>.Failure("One or both snapshots not found.");

        var areEqual = _comparer.Equals(snapshot1.State, snapshot2.State);
        return Result<bool>.Success(areEqual);
    }

    public void ClearSnapshots() => _snapshots.Clear();

    private async Task CleanupOldSnapshotsAsync()
    {
        var cutoff = DateTimeOffset.UtcNow - _retentionTime;
        var oldKeys = _snapshots.Where(kvp => kvp.Value.Timestamp < cutoff).Select(kvp => kvp.Key).ToList();

        await Task.Run(() =>
        {
            foreach (var key in oldKeys)
                _snapshots.TryRemove(key, out _);
        }).ConfigureAwait(false);
    }

    public Result<bool> IsDirty(T currentState)
    {
        try
        {
            if (_snapshots.IsEmpty)
                return Result<bool>.Success(true); // No snapshots available, consider dirty

            if (!_snapshots.TryGetValue(_currentVersion, out var lastSnapshot))
                return Result<bool>.Failure("Failed to retrieve the last snapshot.");

            var isDirty = !_comparer.Equals(lastSnapshot.State, currentState);
            return Result<bool>.Success(isDirty);
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(e.Message);
        }
    }

    public async Task<Result<bool>> IsDirtyAsync(Task<T> currentStateTask)
    {
        try
        {
            var currentState = await currentStateTask.ConfigureAwait(false);
            return IsDirty(currentState);
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(e.Message);
        }
    }

    public T? GetCurrentState() => _currentState is not null ? _currentState.Clone() : default;

    public Result<int> GetCurrentVersion() => Result<int>.Success(_currentVersion);
}
