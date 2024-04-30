namespace DropBear.Codex.StateManagement.DeepCloning.Attributes;

/// <summary>
///     Specifies the preferred cloning method for a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)] // This attribute can only be applied to classes.
public sealed class CloneMethodAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CloneMethodAttribute" /> class.
    /// </summary>
    /// <param name="useExpression">if set to <c>true</c> [use expression-based cloning].</param>
    public CloneMethodAttribute(bool useExpression) => UseExpression = useExpression;

    /// <summary>
    ///     Gets a value indicating whether expression-based cloning should be used.
    /// </summary>
    public bool UseExpression { get; }
}