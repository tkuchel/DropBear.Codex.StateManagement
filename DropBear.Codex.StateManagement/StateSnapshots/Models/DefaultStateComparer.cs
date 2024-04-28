using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;
using Newtonsoft.Json;

namespace DropBear.Codex.StateManagement.StateSnapshots.Models;

public class DefaultStateComparer<T> : IStateComparer<T>
{
    public bool Equals(T x, T y) => JsonConvert.SerializeObject(x) == JsonConvert.SerializeObject(y);

    public int GetHashCode(T obj) => JsonConvert.SerializeObject(obj).GetHashCode(StringComparison.OrdinalIgnoreCase);
}
