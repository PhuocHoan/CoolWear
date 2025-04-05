using System;
using System.Windows.Input;

namespace CoolWear.Utilities;
public partial class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    private readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public partial class RelayCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) =>
        (parameter is T typedParameter && (canExecute?.Invoke(typedParameter) ?? true));

    public void Execute(object? parameter)
    {
        if (parameter is not T typedParameter) return;

        RaiseCanExecuteChanged();

        try
        {
            execute(typedParameter);
        }
        finally
        {
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
