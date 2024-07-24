#region

using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;
using Newtonsoft.Json;

#endregion

namespace DropBear.Codex.StateManagement.StateSnapshots.Models;

public class DefaultStateComparer<T> : IStateComparer<T>
{
    public bool Equals(T x, T y)
    {
        return JsonConvert.SerializeObject(x) == JsonConvert.SerializeObject(y);
    }

    public int GetHashCode(T obj)
    {
        return JsonConvert.SerializeObject(obj).GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
