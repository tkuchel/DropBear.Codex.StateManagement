using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class BulkSnapshotBuilder
{
    private readonly Dictionary<Type, object> _builders = new();
    private ISnapshotManagerRegistry? _registry;

    public BulkSnapshotBuilder UseRegistry(ISnapshotManagerRegistry registry)
    {
        _registry = registry;
        return this;
    }

    public SnapshotBuilder<T> ConfigureFor<T>() where T : ICloneable<T>, new()
    {
        var builder = new SnapshotBuilder<T>();
        _builders[typeof(T)] = builder;
        return builder;
    }

    public void BuildAll()
    {
        foreach (var entry in _builders)
            if (entry.Value is ISnapshotBuilder builder)
            {
                var manager = builder.Build(); // Assuming Build returns a correctly typed manager
                var managerType = manager.GetType();

                // Check if manager is of type StateSnapshotManager<T>
                if (managerType.IsGenericType &&
                    managerType.GetGenericTypeDefinition() == typeof(StateSnapshotManager<>))
                {
                    var typeArgument = managerType.GetGenericArguments()[0]; // This should be ClientUpdateDto

                    // Additional check to ensure that typeArgument implements ICloneable<T>
                    var cloneableInterface = typeof(ICloneable<>).MakeGenericType(typeArgument);
                    if (!cloneableInterface.IsAssignableFrom(typeArgument))
                        throw new InvalidOperationException(
                            $"Type {typeArgument.Name} does not implement ICloneable<{typeArgument.Name}> as expected.");

                    if (_registry is null || string.IsNullOrEmpty(builder.RegistryKey)) continue;
                    
                    Console.WriteLine($"Registering manager of type: {managerType}, with T: {typeArgument}");

                    var method = _registry.GetType().GetMethod(nameof(ISnapshotManagerRegistry.Register));
                    var genericMethod = method.MakeGenericMethod(typeArgument);
                    genericMethod.Invoke(_registry, new[] { manager, builder.RegistryKey });
                }
                else
                {
                    throw new InvalidOperationException(
                        "The manager type is not a generic StateSnapshotManager<T> as expected.");
                }
            }
    }
}
