namespace DropBear.Codex.StateManagement.StateMachine.EventArgs;

public class StateEventArgs<TState> : System.EventArgs where TState : Enum
{
    public StateEventArgs(TState state) => State = state;

    public TState State { get; }
}
