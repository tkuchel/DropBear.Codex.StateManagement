# Enhanced State Machine Example in C#

## Overview
This document provides an example of how to utilize the enhanced `StateManager` and `TimeoutState` in a C# application. It covers the setup of states, including timeout states, and demonstrates how to handle state transitions.

## Enum for States
Define the possible states the application can be in:
```csharp
public enum ExampleState
{
    Idle,
    Running,
    Paused
}
```

## Timeout State Implementation
Here's how you can define a timeout state that inherits from `TimeoutState`:
```csharp
using DropBear.Codex.StateManagement.StateMachine.PredefinedStates;

public class RunningState : TimeoutState<ExampleState>
{
    public RunningState() : base(ExampleState.Running, TimeSpan.FromSeconds(5))
    {
    }

    public override ExampleState GetNextState()
    {
        return ExampleState.Paused;  // Transition to Paused after timeout
    }

    public override void EnterState()
    {
        base.EnterState();
        Console.WriteLine("Running State is active with a 5-second timeout.");
    }

    public override void ExitState()
    {
        base.ExitState();
        Console.WriteLine("Exiting Running State.");
    }
}
```

## Using the StateManager
This section demonstrates how to set up the `StateManager` and manage state transitions, including the handling of timeouts.
```csharp
using DropBear.Codex.StateManagement.StateMachine;

public class StateMachineExample
{
    public static void Main(string[] args)
    {
        var stateManager = new StateManager<ExampleState>();
        stateManager.AddState(ExampleState.Idle, new IdleState());
        stateManager.AddState(ExampleState.Running, new RunningState());
        stateManager.AddState(ExampleState.Paused, new PausedState());

        stateManager.StateTimeout += (sender, e) =>
        {
            Console.WriteLine($"Timeout reached in state: {e.NextState}");
        };

        // Simulate the update process
        stateManager.Update();  // Should transition from Idle to Running
        stateManager.Update();  // Should transition from Running to Paused
        stateManager.Update();  // Should transition from Paused back to Idle
    }
}
```

## Conclusion
This setup provides a robust example of using a state machine with enhanced features such as timeout states. It demonstrates how to handle state transitions effectively, especially when states are meant to transition automatically after a set duration.
