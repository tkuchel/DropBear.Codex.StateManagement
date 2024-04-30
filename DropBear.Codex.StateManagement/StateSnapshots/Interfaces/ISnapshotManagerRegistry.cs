using DropBear.Codex.Core;

namespace DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

public interface ISnapshotManagerRegistry
{
    StateSnapshotManager<T> GetOrCreateManager<T>(string key, bool automaticSnapshotting,
        TimeSpan snapshotInterval, TimeSpan retentionTime) where T : ICloneable<T>;

    StateSnapshotManager<T> GetManager<T>(string key) where T : ICloneable<T>;

    void CreateSnapshot<T>(string key, T currentState) where T : ICloneable<T>;
    Result RevertToSnapshot<T>(string key, int version) where T : ICloneable<T>;
    Result<bool> IsDirty<T>(string key, T currentState) where T : ICloneable<T>;
    void Register<T>(StateSnapshotManager<T> manager, string key) where T : ICloneable<T>;

    void DisposeAll();
}
