using System.Timers;
using DropBear.Codex.StateManagement.StateMachine.EventArgs;
using Timer = System.Timers.Timer;

namespace DropBear.Codex.StateManagement.StateMachine.PredefinedStates;

public class TimeoutState<TState> : BaseState<TState>, IDisposable where TState : Enum
{
    private readonly Timer _timer;

    public TimeoutState(TState key, TimeSpan timeoutDuration) : base(key)
    {
        TimeoutDuration = timeoutDuration;
        _timer = new Timer(timeoutDuration.TotalMilliseconds);
        _timer.Elapsed += OnTimeout;
        _timer.AutoReset = false; // Ensures the timer runs only once
    }

    // Read-only auto-property for Timeout Duration
    public TimeSpan TimeoutDuration { get; }

    public void Dispose() => _timer.Dispose();

    // Event using the EventHandler<TEventArgs> pattern
    public event EventHandler<StateTimeoutEventArgs<TState>>? StateTimeout;

    private void OnTimeout(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop(); // Stop the timer to prevent it from firing again
        OnStateTimeout(); // Raise an event or handle internally
    }

    protected virtual void OnStateTimeout()
    {
        var nextState = GetNextState();
        StateTimeout?.Invoke(this, new StateTimeoutEventArgs<TState>(nextState));
    }

    public override void EnterState()
    {
        Console.WriteLine($"Entering {StateKey} State with timeout: {TimeoutDuration.TotalSeconds} seconds.");
        _timer.Start();
    }

    public override void ExitState()
    {
        Console.WriteLine($"Exiting {StateKey} State.");
        _timer.Stop();
    }

    public override void UpdateState() =>
        Console.WriteLine("Updating state... Define specific behavior in derived classes.");

    public override TState GetNextState()
    {
        Console.WriteLine("Getting next state... Define specific behavior in derived classes.");
        return StateKey; // Default to returning the same state if not overridden
    }

    public override void ResetState()
    {
        _timer.Stop();
        _timer.Start(); // Restart the timer
    }
}
