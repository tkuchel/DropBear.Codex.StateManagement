# DropBear.Codex.StateManagement

DropBear.Codex.StateManagement is a simple state management library designed to monitor and manage the state of your
models efficiently. This library provides functionalities for snapshotting model states, tracking changes, and
efficiently managing model state through a caching system with a sliding expiration policy.

## Features

- **Snapshot Initialization**: Initialize monitoring of a model by taking a snapshot of its state, which is stored in
  the cache with customizable expiration.
- **Change Detection**: Check if the model has changed since the last snapshot was taken and obtain a list of changed
  properties.
- **Snapshot Management**: Clear snapshots from the cache to free up resources, especially for models that are no longer
  in use.
- **Extension Methods**: Includes extension methods to simplify checking and adding properties to models within the
  cache.

## Getting Started

To use DropBear.Codex.StateManagement in your project, follow these steps:

1. Add a reference to the library in your project file.
2. Import the necessary namespaces:

   ```csharp
   using DropBear.Codex.StateManagement.Extensions;
   using DropBear.Codex.StateManagement.Interfaces;
    ```
3. Initialize a snapshot of your model:

   ```csharp
   var model = new YourModel();
   IModelStateSnapshot snapshotManager = new ModelStateSnapshot();
   snapshotManager.InitializeSnapshot(model, TimeSpan.FromMinutes(30));
    ```
4. Check for changes in the model:

   ```csharp
   if (snapshotManager.HasModelChanged(model, out var changes)) {
   Console.WriteLine("Changes detected in the following properties:");
   foreach (var change in changes) {
   Console.WriteLine(change);}}
    ```
5. Clear snapshots from the cache:

    ```csharp
       snapshotManager.ClearSnapshot(model);
    ```

## License

This project is licensed under the LGPLv3 License - see the https://www.gnu.org/licenses/lgpl-3.0.en.html for details.
