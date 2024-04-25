namespace DropBear.Codex.StateManagement.StateMachine.Interfaces;

public interface IState<out TState> where TState : Enum
{
    TState StateKey { get; }
    void EnterState();
    void ExitState();
    void UpdateState();
    TState GetNextState();
    void ResetState(); 
}
