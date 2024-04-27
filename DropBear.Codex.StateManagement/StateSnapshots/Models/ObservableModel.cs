using R3;

namespace DropBear.Codex.StateManagement.StateSnapshots.Models;

public class ObservableModel<T>
{
    private T _state = default!;
    public Subject<T> StateChanged { get; } = new();

    public T State
    {
        get => _state;
        set
        {
            if (!Equals(_state, value))
            {
                _state = value;
                StateChanged.OnNext(value);
            }
        }
    }
}
