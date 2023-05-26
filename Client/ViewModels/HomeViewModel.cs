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

    public int? ChatsWithUnreadMessagesCount => _chat.Chats.Where(c => c.UnreadMessagesCount > 0).Count();

    public ICommand GoProfileCommand { get; }
    public ICommand GoUsersCommand { get; }
    public ICommand GoChatsCommand { get; }
    public ICommand GoSettingCommand { get; }
    public ICommand ExitCommand { get; }

    public HomeViewModel(NavigationStore navigationStore, string token)
    {
        _mainNavigationStore = navigationStore;
        _chat = new ChatService(token);
        _chat.MessageReceived += MessageReceived;
        _homeNavigationStore = new NavigationStore();
        _homeNavigationStore.ViewModelUpdated += HomeViewModelUpdated;
        _homeNavigationStore.CurrentViewModel = new ProfileViewModel(CurrentUser);
        GoProfileCommand = new RelayCommand(GoProfile);
        GoUsersCommand = new RelayCommand(GoUsers);
        GoChatsCommand = new RelayCommand(GoChats);
        GoSettingCommand = new RelayCommand(GoSetting);
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

    private void MessageReceived(Message message)
    {
        RaisePropertyChanged(nameof(ChatsWithUnreadMessagesCount));
    }

    private void HomeViewModelUpdated()
    {
        RaisePropertyChanged(nameof(CurrentViewModel));
        RaisePropertyChanged(nameof(ChatsWithUnreadMessagesCount));
    }

    private void GoProfile(object parameter)
    {
        _homeNavigationStore.CurrentViewModel = new ProfileViewModel(CurrentUser);
    }

    private void GoUsers(object parameter)
    {
        _homeNavigationStore.CurrentViewModel = new UsersListViewModel(_homeNavigationStore, _chat);
    }

    private void GoChats(object parameter)
    {
        _homeNavigationStore.CurrentViewModel = new ChatsListViewModel(_homeNavigationStore, _chat);
    }

    private void GoSetting(object parameter)
    {
        _homeNavigationStore.CurrentViewModel = new SettingViewModel();
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