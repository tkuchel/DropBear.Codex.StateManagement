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
        // Dispose any existing subscription and clear the snapshots
        _snapshots.Clear();
        _stateRevertedSubject.Dispose();
        DisposeCurrentSubscription();
    }

    public Result SubscribeModelChanges(ObservableModel<T> model)
    {
        if (!_automaticSnapshotting)
            return Result.Success(); // Return success if not subscribing due to manual control

        DisposeCurrentSubscription(); // Ensure any existing subscription is disposed

        try
        {
            _subscription = model.StateChanged
                .Debounce(_snapshotInterval, TimeProvider.System)
                .Subscribe(HandleModelChanged);
            
            // Take an initial snapshot immediately upon subscription
            CreateSnapshot(model.State);

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    private void DisposeCurrentSubscription()
    {
        if (_subscription is null) return;
        _subscription.Dispose();
        _subscription = null;
    }

    private void HandleModelChanged(T state) => CreateSnapshot(state);

    public Result CreateSnapshot(T currentState)
    {
        try
        {
            var clonedState = currentState.Clone();
            var snapshot = new Snapshot<T>(clonedState);
            _snapshots[Interlocked.Increment(ref _currentVersion)] = snapshot;
            _currentState = currentState; // Update the current state
            _ = Task.Run(CleanupOldSnapshots); // Opt for Task.Run for simplicity in fire-and-forget scenario
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public Result RevertToSnapshot(int version)
    {
        Console.WriteLine($"Attempting to revert to snapshot version: {version}");
        Console.WriteLine($"Available snapshots: {string.Join(", ", _snapshots.Keys)}");

        if (!_snapshots.TryGetValue(version, out var snapshot)) return Result.Failure("Snapshot not found.");

        _currentState = snapshot.State;
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

        var areEqual = EqualityComparer<T>.Default.Equals(snapshot1.State, snapshot2.State);
        return Result<bool>.Success(areEqual);
    }

    public Result ClearSnapshots()
    {
        try
        {
            _snapshots.Clear();
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
    
    private void CleanupOldSnapshots()
    {
        var cutoff = DateTimeOffset.UtcNow - _retentionTime;
        var oldKeys = _snapshots.Where(kvp => kvp.Value.Timestamp < cutoff).Select(kvp => kvp.Key).ToList();
        foreach (var key in oldKeys)
            _snapshots.TryRemove(key, out _);
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

    public T? GetCurrentState() => _currentState;
}
