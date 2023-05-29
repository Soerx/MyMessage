using Prism.Commands;
using System;
using System.Threading.Tasks;

namespace Client.Commands;

public class AsyncRelayCommand : DelegateCommandBase
{
    private readonly Func<object, Task> _execute;
    private Func<object, bool>? _canExecute;

    public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    protected override bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    protected async override void Execute(object parameter)
    {
        await _execute(parameter);
    }
}