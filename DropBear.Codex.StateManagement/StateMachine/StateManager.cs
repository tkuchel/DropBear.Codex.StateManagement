using DropBear.Codex.StateManagement.StateMachine.EventArgs;
using DropBear.Codex.StateManagement.StateMachine.Interfaces;

namespace DropBear.Codex.StateManagement.StateMachine;

public class StateManager<TState> where TState : Enum
{
    private delegate bool TransitionCondition(TState currentState, TState nextState);

    private readonly Stack<TState> _stateHistory = new();
    private readonly object _stateLock = new();
    private readonly Dictionary<TState, IState<TState>> _states = new();
    private IState<TState>? _currentState;
    private readonly Dictionary<TState, TransitionCondition> _transitionConditions = new();

    public event EventHandler<StateEventArgs<TState>>? BeforeStateChange;
    public event EventHandler<StateEventArgs<TState>>? AfterStateChange;
    public event EventHandler<ErrorEventArgs>? OnError;

    public void Update()
    {
        if (_currentState is null) return;

        var nextState = _currentState.GetNextState();
        // Check if a transition condition exists and evaluates to true
        if (_transitionConditions.TryGetValue(nextState, out var condition) &&
            condition(_currentState.StateKey, nextState)) TransitionToState(nextState);
    }

    private void TransitionToState(TState nextState)
    {
        if (_currentState is null) return;

        try
        {
            BeforeStateChange?.Invoke(this, new StateEventArgs<TState>(_currentState.StateKey));
            _currentState.ExitState();
            _stateHistory.Push(_currentState.StateKey);
            _currentState = _states[nextState];
            _currentState.EnterState();
            AfterStateChange?.Invoke(this, new StateEventArgs<TState>(nextState));
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, new ErrorEventArgs(ex));
        }
    }

    public void RevertToPreviousState()
    {
        if (_stateHistory.Count <= 1) return; // Ensure there is a previous state
        _stateHistory.Pop(); // Discard the current state
        TransitionToState(_stateHistory.Peek()); // Revert to the previous state
    }

    public void ResetToInitialState(TState initialState) => TransitionToState(initialState);

    public void AddState(TState key, IState<TState> state)
    {
        lock (_stateLock)
        {
            _states[key] = state;
            if (!_transitionConditions
                    .ContainsKey(key)) // Ensure every state has at least a default transition condition
                _transitionConditions[key] = (current, next) => true; // Default condition always allows transition
        }
    }
}
