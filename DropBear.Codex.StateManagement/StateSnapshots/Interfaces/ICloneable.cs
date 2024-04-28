namespace DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

public interface ICloneable<out T>
{
    T Clone();
}

