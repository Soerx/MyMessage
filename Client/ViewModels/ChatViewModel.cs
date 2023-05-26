using Client.Commands;
using Client.Models;
using Client.Services;
using Client.Stores;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ChatViewModel : BindableBase, IDisposable
    {
        private bool _subscribed;
        private bool _disposed;
        private readonly NavigationStore _navigationStore;
        private readonly ChatService _chatService;
        private string? _sendedMessageText;
        private string? _sendingMessage;
        
        public Chat? Chat { get; }
        public User? Receiver { get; }

        public string? SendingMessage
        {
            get => _sendingMessage;
            set
            {
                _sendingMessage = value;
                RaisePropertyChanged(nameof(SendingMessage));
            }
        }

        public Visibility ProfileButtonVisibility
        {
            get
            {
                if (Receiver is null)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public ICommand SendMessageCommand { get; }
        public ICommand? GoProfileCommand { get; }

        private ChatViewModel(NavigationStore navigationStore, ChatService chatService)
        {
            _navigationStore = navigationStore;
            _chatService = chatService;
            SendMessageCommand = new RelayCommand(SendMessage);
        }

        public ChatViewModel(NavigationStore navigationStore, ChatService chatService, Chat currentChat) : this(navigationStore, chatService)
        {
            Chat = currentChat;
        }

        public ChatViewModel(NavigationStore navigationStore, ChatService chatService, User receiver) : this(navigationStore, chatService)
        {
            Receiver = receiver;
            GoProfileCommand = new RelayCommand((object parameter) => _navigationStore.CurrentViewModel = new ProfileViewModel(receiver));
        }

        public void Dispose()
        {
            if (_disposed) 
                return;

            _disposed = true;

            if (_subscribed)
                _chatService.MessageReceived -= SwitchToChat;

            GC.SuppressFinalize(this);
        }

        private async void SendMessage(object parameter)
        {
            if (string.IsNullOrWhiteSpace(SendingMessage) == false)
            {
                if (Chat is not null)
                {
                    await _chatService.SendMessage(SendingMessage, Chat);
                }
                else if (Receiver is not null)
                {
                    _chatService.MessageReceived += SwitchToChat;
                    _sendedMessageText = SendingMessage;
                    _subscribed = true;
                    await _chatService.SendMessage(SendingMessage, Receiver);
                }
            }
        }

        private void SwitchToChat(Message message)
        {
            if (App.Instance.CurrentUser is null || _sendedMessageText is null)
                return;

            if (message.Sender.Id == App.Instance.CurrentUser.Id && message.Content.Text == _sendedMessageText)
            {
                _navigationStore.CurrentViewModel = new ChatViewModel(_navigationStore, _chatService, message.Chat);
                Dispose();
            }
        }

        ~ChatViewModel()
        {
            Dispose();
        }
    }
}