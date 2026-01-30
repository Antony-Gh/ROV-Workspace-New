#nullable enable
using System;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _executeWithParam;
    private readonly Func<object?, bool>? _canExecuteWithParam;

    private readonly Action? _executeNoParam;
    private readonly Func<bool>? _canExecuteNoParam;

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _executeNoParam = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecuteNoParam = canExecute;
    }

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecuteWithParam = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecuteNoParam != null)
            return _canExecuteNoParam();

        return _canExecuteWithParam?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (_executeNoParam != null)
            _executeNoParam();
        else
            _executeWithParam?.Invoke(parameter);
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
