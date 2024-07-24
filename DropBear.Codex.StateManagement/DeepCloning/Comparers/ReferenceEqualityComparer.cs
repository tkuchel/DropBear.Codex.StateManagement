#region

using System.Runtime.CompilerServices;

#endregion

namespace DropBear.Codex.StateManagement.DeepCloning.Comparers;

public class ReferenceEqualityComparer : EqualityComparer<object>
{
    public override bool Equals(object? x, object? y)
    {
        return ReferenceEquals(x, y);
    }

    public override int GetHashCode(object obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
}
