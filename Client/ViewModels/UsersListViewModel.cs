using Client.Commands;
using Client.Models;
using Client.Services;
using Client.Stores;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class UsersListViewModel : BindableBase
    {
        private readonly ChatService _chat;
        private readonly NavigationStore _navigationStore;
        private string? _searchText;

        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
                RaisePropertyChanged(nameof(FilteredUsers));
            }
        }

        public ObservableCollection<User> FilteredUsers
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    return _chat.Users;
                }
                else
                {
                    return new ObservableCollection<User>(_chat.Users.Where(u => u.ToString().ToUpper().Contains(SearchText.ToUpper())));
                }
            }
        }

        public ICommand GoToUserCommand { get; }
        public ICommand GoToChatCommand { get; }

        public UsersListViewModel(NavigationStore navigationStore, ChatService chatService)
        {
            _chat = chatService;
            _navigationStore = navigationStore;
            GoToUserCommand = new RelayCommand(GoToUser);
            GoToChatCommand = new RelayCommand(GoToChat);
        }

        private void GoToUser(object parameter)
        {
            if (parameter is User user)
            {
                _navigationStore.CurrentViewModel = new ProfileViewModel(user);
            }
        }

        private void GoToChat(object parameter)
        {
            if (parameter is User user)
            {
                int dialogUsersCount = 2;
                Chat? destinationDialog = _chat.Chats.SingleOrDefault(c => c.Users.Any(u => u.Id == user.Id) && c.Users.Count == dialogUsersCount);

                if (destinationDialog is not null)
                {
                    _navigationStore.CurrentViewModel = new ChatViewModel(_navigationStore, _chat, destinationDialog);
                }
                else
                {
                    _navigationStore.CurrentViewModel = new ChatViewModel(_navigationStore, _chat, user);
                }
            }
        }
    }
}