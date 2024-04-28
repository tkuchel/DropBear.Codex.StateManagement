using System.Runtime.Serialization;

namespace DropBear.Codex.StateManagement.StateSnapshots.Models;

[DataContract]
public class Snapshot<T>
{
    public Snapshot(T state, string createdBy = "")
    {
        State = state;
        Timestamp = DateTimeOffset.UtcNow;
        CreatedBy = ResolveUserName(createdBy);
    }

    protected Snapshot() // For serialization frameworks that require a parameterless constructor
    {
        State = default!;
        Timestamp = default;
        CreatedBy = string.Empty;
    }

    [DataMember(Order = 1)] public T State { get; private set; }

    [DataMember(Order = 2)] public DateTimeOffset Timestamp { get; private set; }

    [DataMember(Order = 3)] public string CreatedBy { get; private set; }

    /// <summary>
    ///     Resolves the username for creating the snapshot, ensuring consistency across different environments.
    /// </summary>
    private static string ResolveUserName(string createdBy)
    {
        if (!string.IsNullOrEmpty(createdBy)) return createdBy;

        // This method can be extended to handle different environments and security contexts.
        // For now, it uses Environment.UserName as a default.
        return Environment.UserName;
    }

    public override string ToString() => $"Snapshot of {typeof(T).Name} taken by {CreatedBy} at {Timestamp}";
}
