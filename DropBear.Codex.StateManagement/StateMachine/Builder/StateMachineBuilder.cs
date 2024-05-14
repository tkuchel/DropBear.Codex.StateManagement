using Stateless;

namespace DropBear.Codex.StateManagement.StateMachine.Builder;

public class StateMachineBuilder<TState, TTrigger>
{
    private readonly StateMachine<TState, TTrigger> _stateMachine;

    public StateMachineBuilder(TState initialState)
    {
        ArgumentNullException.ThrowIfNull(initialState, nameof(initialState));
        _stateMachine = new StateMachine<TState, TTrigger>(initialState);
    }

    public StateMachineBuilder<TState, TTrigger> ConfigureState(TState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        _stateMachine.Configure(state);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> Permit(TTrigger trigger, TState destinationState)
    {
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(destinationState, nameof(destinationState));
        _stateMachine.Configure(destinationState).Permit(trigger, destinationState);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> PermitReentry(TState state, TTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        _stateMachine.Configure(state).PermitReentry(trigger);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> Ignore(TState state, TTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        _stateMachine.Configure(state).Ignore(trigger);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> OnEntry(TState state, Action entryAction)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(entryAction, nameof(entryAction));
        _stateMachine.Configure(state).OnEntry(entryAction);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> OnExit(TState state, Action exitAction)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(exitAction, nameof(exitAction));
        _stateMachine.Configure(state).OnExit(exitAction);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> SubstateOf(TState state, TState superstate)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(superstate, nameof(superstate));
        _stateMachine.Configure(state).SubstateOf(superstate);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> InternalTransition(TState state, TTrigger trigger,
        Action transitionAction)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(transitionAction, nameof(transitionAction));
        _stateMachine.Configure(state).InternalTransition(trigger, transitionAction);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> PermitIf(TState state, TTrigger trigger, TState destinationState,
        Func<bool> guard)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(destinationState, nameof(destinationState));
        ArgumentNullException.ThrowIfNull(guard, nameof(guard));
        _stateMachine.Configure(state).PermitIf(trigger, destinationState, guard);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> InitialTransition(TState state, TState initialState)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(initialState, nameof(initialState));
        _stateMachine.Configure(state).InitialTransition(initialState);
        return this;
    }

    public StateMachineBuilder<TState, TTrigger> OnEntryFrom(TState state, TTrigger trigger,
        Action<StateMachine<TState, TTrigger>.Transition> entryActionFrom)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(entryActionFrom, nameof(entryActionFrom));
        _stateMachine.Configure(state).OnEntryFrom(trigger, entryActionFrom);
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
