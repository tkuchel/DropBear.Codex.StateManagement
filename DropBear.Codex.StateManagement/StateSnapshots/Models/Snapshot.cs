namespace DropBear.Codex.StateManagement.StateSnapshots.Models;

public class Snapshot<T>
{
    public T State { get; private set; }
    public DateTime Timestamp { get; private set; }

    public Snapshot(T state)
    {
        State = state;
        Timestamp = DateTime.UtcNow;
    }
}
