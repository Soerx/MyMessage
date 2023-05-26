using Client.Commands;
using Client.Models;
using Client.Services;
using Client.Stores;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ChatsListViewModel : BindableBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly ChatService _chatService;
        private string? _searchText;

        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
                RaisePropertyChanged(nameof(FilteredChats));
            }
        }

        public ObservableCollection<Chat> FilteredChats
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    return _chatService.Chats;
                }
                else
                {
                    return new ObservableCollection<Chat>(_chatService.Chats.Where(c => c.DisplayName.ToUpper().Contains(SearchText.ToUpper())));
                }
            }
        }

        public ChatService Chat => _chatService;

        public ICommand StartNewChatCommand { get; }
        public ICommand GoToChatCommand { get; }

        public ChatsListViewModel(NavigationStore navigationStore, ChatService chatService)
        {
            _navigationStore = navigationStore;
            _chatService = chatService;
            StartNewChatCommand = new RelayCommand(StartNewChat);
            GoToChatCommand = new RelayCommand(GoToChat);
        }

        private void StartNewChat(object parameter)
        {
            _navigationStore.CurrentViewModel = new UsersListViewModel(_navigationStore, _chatService);
        }

        private void GoToChat(object parameter)
        {
            if (parameter is Chat chat)
            {
                _navigationStore.CurrentViewModel = new ChatViewModel(_navigationStore, _chatService, chat);
            }
        }
    }
}