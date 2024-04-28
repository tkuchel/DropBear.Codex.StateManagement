using System.ComponentModel;
using R3;

namespace DropBear.Codex.StateManagement.StateSnapshots.Models;

public class ObservableModel<T> : INotifyPropertyChanged
{
    private readonly object _lock = new();
    private T _state = default!;

    public Subject<T> StateChanged { get; } = new();

    public T State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
        set
        {
            if (!IsValid(value)) throw new ArgumentException("Invalid state value.", nameof(value));

            lock (_lock)
            {
                if (Equals(_state, value)) return;
                _state = value;
            }

            StateChanged.OnNext(value);
            OnPropertyChanged(nameof(State));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static bool IsValid(T value) => value is not null; // Assume all values are valid by default

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
