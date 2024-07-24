#region

using DropBear.Codex.Core;

#endregion

namespace DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

public interface ISnapshotManagerRegistry
{
    Result<StateSnapshotManager<T>> GetOrCreateManager<T>(string key, bool automaticSnapshotting,
        TimeSpan snapshotInterval, TimeSpan retentionTime, IStateComparer<T> comparer = null!) where T : ICloneable<T>;

    Result<StateSnapshotManager<T>> GetManager<T>(string key) where T : ICloneable<T>;

    Result CreateSnapshot<T>(string key, T currentState) where T : ICloneable<T>;
    Result RevertToSnapshot<T>(string key, int version) where T : ICloneable<T>;
    Result<bool> IsDirty<T>(string key, T currentState) where T : ICloneable<T>;
    Result Register<T>(StateSnapshotManager<T> manager, string key, bool overwrite = false) where T : ICloneable<T>;
    Task<Result> CreateSnapshotAsync<T>(string key, Task<T> currentStateTask) where T : ICloneable<T>;
    Task<Result<bool>> IsDirtyAsync<T>(string key, Task<T> currentStateTask) where T : ICloneable<T>;

    void DisposeAll();
}
