namespace DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

public interface ISnapshotBuilder
{
    object Build();
    string RegistryKey { get; }
}

