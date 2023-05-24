using Client.Commands;
using Client.Models;
using Client.Services;
using Client.Stores;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels;

public class HomeViewModel : BindableBase, IDisposable
{
    private bool _disposed;
    private readonly ChatService _chat;
    private readonly NavigationStore _mainNavigationStore;
    private readonly NavigationStore _homeNavigationStore;

    public User? CurrentUser => App.Instance.CurrentUser;
    public BindableBase? CurrentViewModel => _homeNavigationStore.CurrentViewModel;

    public int? UnreadMessagesCount => _chat.Messages.Where(m => m.IsRead == false).Count();

    public ICommand GoProfileCommand { get; }
    public ICommand GoUsersCommand { get; }
    public ICommand ExitCommand { get; }

    public HomeViewModel(NavigationStore navigationStore, string token)
    {
        _mainNavigationStore = navigationStore;
        _chat = new ChatService(token);
        _homeNavigationStore = new NavigationStore();
        _homeNavigationStore.ViewModelUpdated += HomeViewModelUpdated;
        _homeNavigationStore.CurrentViewModel = new ProfileViewModel(CurrentUser);
        GoProfileCommand = new RelayCommand(GoProfile);
        GoUsersCommand = new RelayCommand(GoUsers);
        ExitCommand = new RelayCommand(Exit);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        
        _disposed = true;
        _homeNavigationStore.ViewModelUpdated -= HomeViewModelUpdated;
        GC.SuppressFinalize(this);
    }

    private void HomeViewModelUpdated()
    {
        RaisePropertyChanged(nameof(CurrentViewModel));
    }

    private void GoProfile(object parameter)
    {
        _homeNavigationStore.CurrentViewModel = new ProfileViewModel(CurrentUser);
    }

    private void GoUsers(object parameter)
    {
        _homeNavigationStore.CurrentViewModel = new UsersViewModel(_homeNavigationStore, _chat);
    }

    private void Exit(object parameter)
    {
        _mainNavigationStore.CurrentViewModel = new AuthViewModel(_mainNavigationStore);
        _chat.Dispose();
        Dispose();
    }

    ~HomeViewModel()
    {
        Dispose();
    }
}