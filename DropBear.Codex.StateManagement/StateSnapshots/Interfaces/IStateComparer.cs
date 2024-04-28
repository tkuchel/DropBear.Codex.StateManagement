namespace DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

public interface IStateComparer<in T>
{
    bool Equals(T x, T y);
    int GetHashCode(T obj);
}

