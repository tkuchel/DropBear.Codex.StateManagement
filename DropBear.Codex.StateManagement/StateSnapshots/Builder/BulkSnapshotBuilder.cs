using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class BulkSnapshotBuilder : IBulkSnapshotBuilder
{
    private readonly Dictionary<Type, ISnapshotBuilder> _builders = new();
    private ISnapshotManagerRegistry? _registry;

    public IBulkSnapshotBuilder WithRegistry(ISnapshotManagerRegistry registry)
    {
        _registry = registry;
        return this;
    }

    public ISnapshotBuilder ConfigureFor<T>() where T : ICloneable<T>, new()
    {
        if (!_builders.TryGetValue(typeof(T), out var builder))
        {
            builder = new SnapshotBuilder<T>();
            _builders[typeof(T)] = builder;
        }

        if (_registry is not null && builder is ISnapshotBuilder<T> typedBuilder)
            typedBuilder.WithRegistry(_registry, typeof(T).FullName!);

        return (ISnapshotBuilder<T>)builder;
    }

    public Result BuildAll()
    {
        try
        {
            foreach (var builder in _builders.Values)
            {
                var manager = builder.Build();
                var managerType = manager.GetType();

                if (managerType.IsGenericType &&
                    managerType.GetGenericTypeDefinition() == typeof(StateSnapshotManager<>))
                {
                    var typeArgument = managerType.GetGenericArguments()[0];

                    if (!typeof(ICloneable<>).MakeGenericType(typeArgument).IsAssignableFrom(typeArgument))
                        return Result.Failure(
                            $"Type {typeArgument.Name} does not implement ICloneable<{typeArgument.Name}> as expected.");

                    if (_registry is null || string.IsNullOrEmpty(builder.RegistryKey)) continue;

                    var registerMethod =
                        typeof(ISnapshotManagerRegistry).GetMethod(nameof(ISnapshotManagerRegistry.Register));
                    var genericRegisterMethod = registerMethod?.MakeGenericMethod(typeArgument);

                    var registrationResult = (Result)genericRegisterMethod?.Invoke(_registry,
                        [manager, builder.RegistryKey, false])!;

                    if (!registrationResult.IsSuccess) return registrationResult;
                }
                else
                {
                    return Result.Failure("The manager type is not a generic StateSnapshotManager<T> as expected.");
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while building snapshot managers: {ex.Message}", ex);
        }
    }
}
