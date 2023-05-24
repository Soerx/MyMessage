using Client.Commands;
using Client.Models;
using Client.Services;
using Client.Stores;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class UsersViewModel : BindableBase
    {
        private readonly ChatService _chat;
        private readonly NavigationStore _navigationStore;

        public ObservableCollection<User> Users => _chat.Users;

        public ICommand GoToUserCommand { get; }

        public UsersViewModel(NavigationStore navigationStore, ChatService chatService)
        {
            _chat = chatService;
            _navigationStore = navigationStore;
            GoToUserCommand = new RelayCommand(GoToUser);
        }

        private void GoToUser(object parameter)
        {
            if (parameter is User user)
            {
                _navigationStore.CurrentViewModel = new ProfileViewModel(user);
            }
        }
    }
}