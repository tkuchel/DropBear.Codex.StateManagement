namespace DropBear.Codex.StateManagement.StateMachine.EventArgs;

// Custom EventArgs to carry state information
public class StateTimeoutEventArgs<TState> : System.EventArgs where TState : Enum
{
    public StateTimeoutEventArgs(TState nextState) => NextState = nextState;

    public TState NextState { get; }
}
