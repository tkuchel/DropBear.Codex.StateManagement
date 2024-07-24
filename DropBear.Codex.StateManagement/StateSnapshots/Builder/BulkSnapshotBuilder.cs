#region

using DropBear.Codex.Core;
using DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

#endregion

namespace DropBear.Codex.StateManagement.StateSnapshots.Builder;

public class BulkSnapshotBuilder : IBulkSnapshotBuilder
{
    private readonly Dictionary<Type, ISnapshotBuilder?> _builders = new();
    private ISnapshotManagerRegistry? _registry;

    public IBulkSnapshotBuilder WithRegistry(ISnapshotManagerRegistry registry)
    {
        _registry = registry;
        return this;
    }

    public ISnapshotBuilder<T>? ConfigureFor<T>() where T : ICloneable<T>, new()
    {
        if (!_builders.TryGetValue(typeof(T), out var builder))
        {
            builder = new SnapshotBuilder<T>();
            _builders[typeof(T)] = builder;
        }

        if (_registry is not null && builder is ISnapshotBuilder<T> typedBuilder)
        {
            typedBuilder.WithRegistry(_registry, typeof(T).FullName!);
        }

        return builder as ISnapshotBuilder<T> ?? default;
    }

    public Result BuildAll()
    {
        var hasPartialSuccess = false;
        var hasWarning = false;
        List<string> errors = [];

        try
        {
            foreach (var builder in _builders.Values)
            {
                if (builder is null)
                {
                    hasWarning = true;
                    Console.WriteLine("Warning: Encountered a null builder.");
                    continue;
                }

                var managerResult = builder.Build();
                if (!managerResult.IsSuccess)
                {
                    if (managerResult.ErrorMessage is not null)
                    {
                        errors.Add(managerResult.ErrorMessage);
                        Console.WriteLine($"Error: {managerResult.ErrorMessage}");
                    }

                    continue; // Consider how to handle partial failures
                }

                var manager = managerResult.Value;
                var managerType = manager?.GetType();
                if (managerType == null)
                {
                    hasWarning = true;
                    Console.WriteLine("Warning: Manager type could not be determined.");
                    continue;
                }

                if (managerType.IsGenericType &&
                    managerType.GetGenericTypeDefinition() == typeof(StateSnapshotManager<>))
                {
                    var typeArgument = managerType.GetGenericArguments()[0];
                    if (!typeof(ICloneable<>).MakeGenericType(typeArgument).IsAssignableFrom(typeArgument))
                    {
                        errors.Add(
                            $"Type {typeArgument.Name} does not implement ICloneable<{typeArgument.Name}> as expected.");
                        continue;
                    }

                    if (_registry is null || string.IsNullOrEmpty(builder.RegistryKey))
                    {
                        hasWarning = true;
                        Console.WriteLine(
                            $"Warning: Registry or registry key is null or empty for {typeArgument.Name}.");
                        continue;
                    }

                    var registerMethod =
                        typeof(ISnapshotManagerRegistry).GetMethod(nameof(ISnapshotManagerRegistry.Register));
                    var genericRegisterMethod = registerMethod?.MakeGenericMethod(typeArgument);
                    if (genericRegisterMethod == null)
                    {
                        hasWarning = true;
                        Console.WriteLine($"Warning: Unable to find register method for {typeArgument.Name}.");
                        continue;
                    }

                    var registrationResult =
                        (Result)genericRegisterMethod.Invoke(_registry, new[] { manager, builder.RegistryKey, true })!;
                    if (registrationResult is { IsSuccess: false })
                    {
                        errors.Add(registrationResult.ErrorMessage);
                        Console.WriteLine(
                            $"Registration failed for {typeArgument.Name}: {registrationResult.ErrorMessage}");
                    }
                    else
                    {
                        hasPartialSuccess = true;
                    }
                }
                else
                {
                    hasWarning = true;
                    Console.WriteLine(
                        "Warning: The manager type is not a generic StateSnapshotManager<T> as expected.");
                }
            }

            if (errors.Count > 0)
            {
                return Result.Failure("Errors occurred during build process.",
                    new AggregateException(errors.Select(e => new InvalidOperationException(e))));
            }

            if (hasWarning)
            {
                return Result.Warning("Build completed with warnings.");
            }

            return hasPartialSuccess
                ? Result.PartialSuccess("Some components were only partially successful.")
                : Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An unhandled exception occurred: {ex.Message}", ex);
        }
    }
}
