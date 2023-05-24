using Client.Stores;
using Prism.Mvvm;
using System;

namespace Client.ViewModels;

public class MainViewModel : BindableBase, IDisposable
{
    private bool _disposed;
    private readonly NavigationStore _navigationStore;

    public BindableBase? CurrentViewModel => _navigationStore.CurrentViewModel;

    public MainViewModel()
    {
        _navigationStore = new NavigationStore();
        _navigationStore.ViewModelUpdated += CurrentViewModelUpdated;
        _navigationStore.CurrentViewModel = new AuthViewModel(_navigationStore);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _navigationStore.ViewModelUpdated -= CurrentViewModelUpdated;
        GC.SuppressFinalize(this);
    }

    private void CurrentViewModelUpdated()
    {
        RaisePropertyChanged(nameof(CurrentViewModel));
    }

    ~MainViewModel()
    {
        Dispose();
    }
}