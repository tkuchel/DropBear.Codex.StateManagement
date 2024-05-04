using DropBear.Codex.Core;

namespace DropBear.Codex.StateManagement.StateSnapshots.Interfaces;

public interface ISnapshotBuilder
{
    /// <summary>
    /// Builds the snapshot manager.
    /// </summary>
    /// <returns>A result containing the built snapshot manager if successful, or a failure result if an error occurred.</returns>
    Result<object> Build();

    /// <summary>
    /// Gets the registry key associated with the snapshot builder.
    /// </summary>
    string? RegistryKey { get; }
}

public interface ISnapshotBuilder<T> : ISnapshotBuilder where T : ICloneable<T>
{
    /// <summary>
    /// Configures the snapshot builder to enable or disable automatic snapshotting.
    /// </summary>
    /// <param name="enabled">Specifies whether automatic snapshotting should be enabled.</param>
    /// <returns>The snapshot builder instance for chaining configuration methods.</returns>
    ISnapshotBuilder<T> WithAutomaticSnapshotting(bool enabled);

    /// <summary>
    /// Configures the snapshot interval for automatic snapshotting.
    /// </summary>
    /// <param name="interval">The time interval between automatic snapshots.</param>
    /// <returns>The snapshot builder instance for chaining configuration methods.</returns>
    ISnapshotBuilder<T> WithSnapshotInterval(TimeSpan interval);

    /// <summary>
    /// Configures the retention time for snapshots.
    /// </summary>
    /// <param name="retentionTime">The time duration for which snapshots should be retained.</param>
    /// <returns>The snapshot builder instance for chaining configuration methods.</returns>
    ISnapshotBuilder<T> WithRetentionTime(TimeSpan retentionTime);

    /// <summary>
    /// Configures the snapshot manager registry and registry key for the snapshot builder.
    /// </summary>
    /// <param name="registry">The snapshot manager registry to be used.</param>
    /// <param name="registryKey">The registry key associated with the snapshot builder.</param>
    /// <returns>The snapshot builder instance for chaining configuration methods.</returns>
    ISnapshotBuilder<T> WithRegistry(ISnapshotManagerRegistry registry, string? registryKey = null);

    /// <summary>
    /// Configures the state comparer for comparing snapshots.
    /// </summary>
    /// <param name="comparer">The state comparer to be used for comparing snapshots.</param>
    /// <returns>The snapshot builder instance for chaining configuration methods.</returns>
    ISnapshotBuilder<T> WithComparer(IStateComparer<T>? comparer);

    /// <summary>
    /// Builds the snapshot manager for the specified type.
    /// </summary>
    /// <returns>A result containing the built snapshot manager if successful, or a failure result if an error occurred.</returns>
    new Result<StateSnapshotManager<T>> Build();
}