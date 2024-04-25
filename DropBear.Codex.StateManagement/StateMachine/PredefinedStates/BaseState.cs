using DropBear.Codex.StateManagement.StateMachine.Interfaces;

namespace DropBear.Codex.StateManagement.StateMachine.PredefinedStates;

public abstract class BaseState<TState> : IState<TState> where TState : Enum
{
    protected BaseState(TState key) => StateKey = key;
    public TState StateKey { get; }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract TState GetNextState();
    public abstract void ResetState();
}
