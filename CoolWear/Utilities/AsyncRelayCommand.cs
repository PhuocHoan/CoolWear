﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.Utilities;

public partial class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
    private bool _isExecuting; // Để tránh tái nhập

    public bool CanExecute(object? parameter) =>
        !_isExecuting && (canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        if (_isExecuting) return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await execute();
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public partial class AsyncRelayCommand<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null) : ICommand
{
    private bool _isExecuting; // Để tránh tái nhập

    public bool CanExecute(object? parameter) =>
        !_isExecuting && (parameter is T typedParameter && (canExecute?.Invoke(typedParameter) ?? true));

    public async void Execute(object? parameter)
    {
        if (_isExecuting || parameter is not T typedParameter) return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await execute(typedParameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}