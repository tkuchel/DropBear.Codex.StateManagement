# StateSnapshotManager Library

## Overview

The `StateSnapshotManager` library provides a comprehensive solution for managing state snapshots in .NET applications. It supports automatic snapshotting, state reversion, and notifications upon state changes, making it ideal for applications that require historical state management or undo capabilities.

## Features

- **Automatic Snapshotting**: Automatically captures snapshots of your application's state at configured intervals.
- **State Reversion**: Allows reverting to any previously captured state snapshot.
- **Observable State Changes**: Utilizes the R3 library to notify subscribers about state reversions, enabling reactive programming scenarios.
- **Flexible Configuration**: Use the `SnapshotBuilder` for easy and fluent configuration of snapshot managers.
- **Multi-Model Management**: Manage snapshots for multiple models using the `SnapshotManagerRegistry`.

## Getting Started

### Installation

To install the `StateSnapshotManager` library, use the following NuGet command:

```bash
Install-Package DropBear.Codex.StateManagement
```

### Usage

Here's a quick example to get you started with a basic snapshot manager:

```csharp
using DropBear.Codex.StateManagement.StateSnapshots;

public class YourApplication
{
    public void Setup()
    {
        var snapshotManager = new StateSnapshotManager<MyStateType>(true, TimeSpan.FromMinutes(5), TimeSpan.FromDays(1));
        snapshotManager.StateReverted.Subscribe(state =>
        {
            Console.WriteLine("State has been reverted.");
        });

        // Assume `currentState` is an instance of `MyStateType`
        snapshotManager.CreateSnapshot(currentState);
    }
}
```

#### Using the SnapshotBuilder

Here’s how to use the `SnapshotBuilder` to create a configured `StateSnapshotManager`:

```csharp
var builder = new SnapshotBuilder<MyStateType>()
    .SetAutomaticSnapshotting(true)
    .SetSnapshotInterval(TimeSpan.FromMinutes(10))
    .SetRetentionTime(TimeSpan.FromDays(7));

var manager = builder.Build();
```

#### Using the SnapshotManagerRegistry

To manage multiple types of snapshot managers:

```csharp
var registry = new SnapshotManagerRegistry();
registry.CreateSnapshot("userManager", new User { Name = "Alice", Age = 30 });
registry.CreateSnapshot("productManager", new Product { Name = "Widget", Price = 19.99 });

// Reverting state for a user manager
var result = registry.RevertToSnapshot<User>("userManager", 1);
```

## Configuration

`StateSnapshotManager` can be configured with the following parameters:

- `automaticSnapshotting`: Whether the manager should automatically take snapshots.
- `snapshotInterval`: The time interval between automatic snapshots.
- `retentionTime`: How long snapshots should be retained before being discarded.

## Building and Contributing

Contributions to the library are welcome! To build the project from source, clone the repository and open it in your preferred .NET development environment.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and further assistance, contact the package maintainer or submit an issue on the GitHub repository.
