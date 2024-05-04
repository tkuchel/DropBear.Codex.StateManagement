using DropBear.Codex.Core;

namespace DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

public interface IBulkSnapshotBuilder
{
    IBulkSnapshotBuilder WithRegistry(ISnapshotManagerRegistry registry);
    ISnapshotBuilder<T>? ConfigureFor<T>() where T : ICloneable<T>, new();
    Result BuildAll();
}
