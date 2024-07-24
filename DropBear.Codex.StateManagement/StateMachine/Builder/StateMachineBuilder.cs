#region

using Stateless;

#endregion

namespace DropBear.Codex.StateManagement.StateMachine.Builder;

/// <summary>
///     A fluent builder for configuring a state machine.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TTrigger">The type of the trigger.</typeparam>
public class StateMachineBuilder<TState, TTrigger>
{
    private readonly StateMachine<TState, TTrigger> _stateMachine;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StateMachineBuilder{TState,TTrigger}" /> class.
    /// </summary>
    /// <param name="initialState">The initial state of the state machine.</param>
    public StateMachineBuilder(TState initialState)
    {
        ArgumentNullException.ThrowIfNull(initialState, nameof(initialState));
        _stateMachine = new StateMachine<TState, TTrigger>(initialState);
    }

    /// <summary>
    ///     Configures the specified state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <returns>The state configuration for further chaining.</returns>
    public StateMachine<TState, TTrigger>.StateConfiguration ConfigureState(TState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        return _stateMachine.Configure(state);
    }

    /// <summary>
    ///     Configures the state machine to permit transitioning from the specified state to the destination state when the
    ///     specified trigger is fired.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="destinationState">The destination state.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> Permit(TState state, TTrigger trigger, TState destinationState)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(destinationState, nameof(destinationState));
        _stateMachine.Configure(state).Permit(trigger, destinationState);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to permit reentry into the specified state when the specified trigger is fired.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger that causes the reentry.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> PermitReentry(TState state, TTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        _stateMachine.Configure(state).PermitReentry(trigger);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to permit reentry into the specified state when the specified trigger is fired,
    ///     executing the specified action.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger that causes the reentry.</param>
    /// <param name="action">The action to execute during reentry.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> PermitReentry(TState state, TTrigger trigger, Action action)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        _stateMachine.Configure(state).PermitReentry(trigger).OnEntry(action);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to ignore the specified trigger when in the specified state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger to ignore.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> Ignore(TState state, TTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        _stateMachine.Configure(state).Ignore(trigger);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to ignore the specified trigger when in the specified state, if the specified guard
    ///     condition is met.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger to ignore.</param>
    /// <param name="guard">The guard condition that must be met for the trigger to be ignored.</param>
    /// <param name="guardDescription">The description of the guard condition.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> IgnoreIf(TState state, TTrigger trigger, Func<bool> guard,
        string? guardDescription = null)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(guard, nameof(guard));
        _stateMachine.Configure(state).IgnoreIf(trigger, guard, guardDescription);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to ignore the specified trigger when in the specified state, if any of the specified
    ///     guard conditions are met.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger to ignore.</param>
    /// <param name="guards">The guard conditions that must be met for the trigger to be ignored.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> IgnoreIf(TState state, TTrigger trigger,
        params Tuple<Func<bool>, string>[] guards)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(guards, nameof(guards));
        _stateMachine.Configure(state).IgnoreIf(trigger, guards);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to execute the specified action when entering the specified state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="entryAction">The action to execute when entering the state.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> OnEntry(TState state, Action entryAction)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(entryAction, nameof(entryAction));
        _stateMachine.Configure(state).OnEntry(entryAction);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to execute the specified action when entering the specified state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="entryAction">The action to execute when entering the state.</param>
    /// <param name="entryActionDescription">The description of the entry action.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> OnEntry(TState state,
        Action<StateMachine<TState, TTrigger>.Transition> entryAction, string? entryActionDescription = null)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(entryAction, nameof(entryAction));
        _stateMachine.Configure(state).OnEntry(entryAction, entryActionDescription);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to execute the specified action when exiting the specified state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="exitAction">The action to execute when exiting the state.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> OnExit(TState state, Action exitAction)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(exitAction, nameof(exitAction));
        _stateMachine.Configure(state).OnExit(exitAction);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to execute the specified action when exiting the specified state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="exitAction">The action to execute when exiting the state.</param>
    /// <param name="exitActionDescription">The description of the exit action.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> OnExit(TState state,
        Action<StateMachine<TState, TTrigger>.Transition> exitAction, string? exitActionDescription = null)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(exitAction, nameof(exitAction));
        _stateMachine.Configure(state).OnExit(exitAction, exitActionDescription);
        return this;
    }

    /// <summary>
    ///     Configures the specified state as a substate of the specified superstate.
    /// </summary>
    /// <param name="state">The substate to configure.</param>
    /// <param name="superstate">The superstate.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> SubstateOf(TState state, TState superstate)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(superstate, nameof(superstate));
        _stateMachine.Configure(state).SubstateOf(superstate);
        return this;
    }

    /// <summary>
    ///     Configures the state machine with an internal transition for the specified state and trigger, executing the
    ///     specified action.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger that causes the internal transition.</param>
    /// <param name="transitionAction">The action to execute during the internal transition.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> InternalTransition(TState state, TTrigger trigger,
        Action<StateMachine<TState, TTrigger>.Transition> transitionAction)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(transitionAction, nameof(transitionAction));
        _stateMachine.Configure(state).InternalTransition(trigger, transitionAction);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to permit transitioning from the specified state to the destination state when the
    ///     specified trigger is fired, if the specified guard condition is met.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="destinationState">The destination state.</param>
    /// <param name="guard">The guard condition that must be met for the transition to occur.</param>
    /// <param name="guardDescription">The description of the guard condition.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> PermitIf(TState state, TTrigger trigger, TState destinationState,
        Func<bool> guard, string? guardDescription = null)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(destinationState, nameof(destinationState));
        ArgumentNullException.ThrowIfNull(guard, nameof(guard));
        _stateMachine.Configure(state).PermitIf(trigger, destinationState, guard, guardDescription);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to permit transitioning from the specified state to the destination state when the
    ///     specified trigger is fired, if all of the specified guard conditions are met.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="destinationState">The destination state.</param>
    /// <param name="guards">The guard conditions that must be met for the transition to occur.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> PermitIf(TState state, TTrigger trigger, TState destinationState,
        params Tuple<Func<bool>, string>[] guards)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(destinationState, nameof(destinationState));
        ArgumentNullException.ThrowIfNull(guards, nameof(guards));
        _stateMachine.Configure(state).PermitIf(trigger, destinationState, guards);
        return this;
    }

    /// <summary>
    ///     Configures the state machine with an initial transition to the specified state when entering the specified state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="initialState">The initial state to transition to.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> InitialTransition(TState state, TState initialState)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(initialState, nameof(initialState));
        _stateMachine.Configure(state).InitialTransition(initialState);
        return this;
    }

    /// <summary>
    ///     Configures the state machine to execute the specified action when entering the specified state via the specified
    ///     trigger.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="entryAction">The action to execute when entering the state via the specified trigger.</param>
    /// <param name="entryActionDescription">The description of the entry action.</param>
    /// <returns>The state machine builder for fluent chaining.</returns>
    public StateMachineBuilder<TState, TTrigger> OnEntryFrom(TState state, TTrigger trigger,
        Action<StateMachine<TState, TTrigger>.Transition> entryAction, string? entryActionDescription = null)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(trigger, nameof(trigger));
        ArgumentNullException.ThrowIfNull(entryAction, nameof(entryAction));
        _stateMachine.Configure(state).OnEntryFrom(trigger, entryAction, entryActionDescription);
        return this;
    }

    /// <summary>
    ///     Builds and returns the configured state machine.
    /// </summary>
    /// <returns>The configured state machine.</returns>
    public StateMachine<TState, TTrigger> Build()
    {
        return _stateMachine;
    }
}
