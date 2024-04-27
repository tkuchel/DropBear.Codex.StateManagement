using Stateless;

namespace DropBear.Codex.StateManagement.StateMachine.Builder;

public class StateMachineBuilder<TState, TTrigger>
{
    private readonly StateMachine<TState, TTrigger> _stateMachine;

    public StateMachineBuilder(TState initialState)
    {
        if (initialState is null)
            throw new ArgumentNullException(nameof(initialState), "Initial state cannot be null.");
        _stateMachine = new StateMachine<TState, TTrigger>(initialState);
    }

    public StateMachineBuilder<TState, TTrigger> ConfigureState(TState state)
    {
        if (state is null) throw new ArgumentNullException(nameof(state), "State cannot be null.");
        _stateMachine.Configure(state);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> Permit(TTrigger trigger, TState destinationState)
    {
        if (trigger is null) throw new ArgumentNullException(nameof(trigger), "Trigger cannot be null.");
        if (destinationState is null)
            throw new ArgumentNullException(nameof(destinationState), "Destination state cannot be null.");
        _stateMachine.Configure(destinationState).Permit(trigger, destinationState);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> OnEntry(TState state, Action entryAction)
    {
        if (state is null) throw new ArgumentNullException(nameof(state), "State cannot be null.");
        if (entryAction is null) throw new ArgumentNullException(nameof(entryAction), "Entry action cannot be null.");
        _stateMachine.Configure(state).OnEntry(entryAction);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> OnExit(TState state, Action exitAction)
    {
        if (state is null) throw new ArgumentNullException(nameof(state), "State cannot be null.");
        if (exitAction is null) throw new ArgumentNullException(nameof(exitAction), "Exit action cannot be null.");
        _stateMachine.Configure(state).OnExit(exitAction);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> SubstateOf(TState state, TState superstate)
    {
        if (state is null) throw new ArgumentNullException(nameof(state), "State cannot be null.");
        if (superstate is null) throw new ArgumentNullException(nameof(superstate), "Superstate cannot be null.");
        _stateMachine.Configure(state).SubstateOf(superstate);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> InternalTransition(TState state, TTrigger trigger, Action transitionAction)
    {
        if (state is null) throw new ArgumentNullException(nameof(state), "State cannot be null.");
        if (trigger is null) throw new ArgumentNullException(nameof(trigger), "Trigger cannot be null.");
        if (transitionAction is null) throw new ArgumentNullException(nameof(transitionAction), "Transition action cannot be null.");
        _stateMachine.Configure(state).InternalTransition(trigger, transitionAction);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> PermitIf(TState state, TTrigger trigger, TState destinationState, Func<bool> guard)
    {
        if (state is null) throw new ArgumentNullException(nameof(state), "State cannot be null.");
        if (trigger is null) throw new ArgumentNullException(nameof(trigger), "Trigger cannot be null.");
        if (destinationState is null) throw new ArgumentNullException(nameof(destinationState), "Destination state cannot be null.");
        if (guard is null) throw new ArgumentNullException(nameof(guard), "Guard condition cannot be null.");
        _stateMachine.Configure(state).PermitIf(trigger, destinationState, guard);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> InitialTransition(TState state, TState initialState)
    {
        if (state is null) throw new ArgumentNullException(nameof(state), "State cannot be null.");
        if (initialState is null)
            throw new ArgumentNullException(nameof(initialState), "Initial state cannot be null.");
        _stateMachine.Configure(state).InitialTransition(initialState);
        return this;
    }

    public StateMachine<TState, TTrigger> Build() => _stateMachine;
}

// Usage example to demonstrate the enhanced features:
// public class StateMachineExample
// {
//     public void ExampleUsage()
//     {
//         var builder = new StateMachineBuilder<State, Trigger>(State.OffHook);
//         var phoneCall = builder
//             .ConfigureState(State.OffHook)
//             .Permit(Trigger.CallDialled, State.Ringing)
//             .ConfigureState(State.Connected)
//             .OnEntry(State.Connected, StartCallTimer)
//             .OnExit(State.Connected, StopCallTimer)
//             .SubstateOf(State.OnHold, State.Connected)
//             .PermitIf(State.OffHook, Trigger.LeftMessage, State.OffHook, () => IsValidNumber())
//             .InternalTransition(State.Connected, Trigger.MuteMicrophone, OnMute)
//             .InitialTransition(State.Connected, State.OnHold)
//             .Build();
//
//         phoneCall.Fire(Trigger.CallDialled);
//         Console.WriteLine($"Current State: {phoneCall.State}");
//     }
//
//     private void StartCallTimer()
//     {
//         Console.WriteLine("Call Timer Started.");
//     }
//
//     private void StopCallTimer()
//     {
//         Console.WriteLine("Call Timer Stopped.");
//     }
//
//     private void OnMute()
//     {
//         Console.WriteLine("Microphone Muted.");
//     }
//
//     private bool IsValidNumber()
//     {
//         // Assume it's always valid for this example
//         return true;
//     }
// }
//
// public enum State
// {
//     OffHook,          // The phone is on the hook
//     Ringing,          // The phone is ringing
//     Connected,        // In a call
//     OnHold,           // The call is on hold
//     PhoneDestroyed    // The phone is destroyed (e.g., thrown against a wall)
// }
//
// public enum Trigger
// {
//     CallDialled,      // Trigger to start the call
//     HangUp,           // Trigger to end the call
//     CallConnected,    // Trigger when the call is answered
//     PlacedOnHold,     // Trigger when the call is placed on hold
//     TakenOffHold,     // Trigger when the call is taken off hold
//     LeftMessage,      // Trigger when leaving a voicemail
//     MuteMicrophone,   // Trigger to mute the call
//     UnmuteMicrophone, // Trigger to unmute the call
//     PhoneHurledAgainstWall // Trigger when the phone is destroyed
// }

