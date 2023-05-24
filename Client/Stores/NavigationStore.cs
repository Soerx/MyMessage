using Prism.Mvvm;
using System;

namespace Client.Stores;

public class NavigationStore
{
    private BindableBase? _currentViewModel;

    public event Action? ViewModelUpdated;

    public BindableBase? CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            if (_currentViewModel is IDisposable disposableVM)
                disposableVM.Dispose();

            _currentViewModel = value;
            ViewModelUpdated?.Invoke();
        }
    }
}