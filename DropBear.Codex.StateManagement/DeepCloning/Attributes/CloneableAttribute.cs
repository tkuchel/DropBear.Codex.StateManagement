namespace DropBear.Codex.StateManagement.DeepCloning.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class CloneableAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CloneableAttribute" /> class.
    /// </summary>
    /// <param name="isCloneable">If set to <see langword="true"/>, the field is considered cloneable; otherwise, it is not.</param>
    public CloneableAttribute(bool isCloneable = true) => IsCloneable = isCloneable;

    /// <summary>
    ///     Gets or sets a value indicating whether the field is cloneable.
    /// </summary>
    public bool IsCloneable { get; }
}
