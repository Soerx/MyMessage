using Client.Commands;
using Client.Controls;
using Client.Models;
using Client.Services;
using Client.Stores;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels;

public class ChatViewModel : BindableBase, IDisposable
{
    private readonly NavigationStore _navigationStore;
    private readonly ChatService _chatService;
    private string? _sendingMessage;
    private bool _disposed;

    public User Receiver { get; }

    public string? SendingMessage
    {
        get => _sendingMessage;
        set
        {
            _sendingMessage = value;
            RaisePropertyChanged(nameof(SendingMessage));
        }
    }

    public ObservableCollection<MessageControl> Messages => new(_chatService.MessagesControls
            .Where(mC => mC.MessageViewModel.Message.SenderUsername == Receiver.Username
            || mC.MessageViewModel.Message.ReceiverUsername == Receiver.Username));

    public ICommand SendMessageCommand { get; }
    public ICommand? GoProfileCommand { get; }

    public ChatViewModel(NavigationStore navigationStore, ChatService chatService, User receiver)
    {
        _navigationStore = navigationStore;
        _chatService = chatService;
        Receiver = receiver;
        SendMessageCommand = new RelayCommand(SendMessage);
        GoProfileCommand = new RelayCommand(GoToUser);
        _chatService.MessageReceived += UpdateReceivedMessages;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _chatService.MessageReceived -= UpdateReceivedMessages;
        GC.SuppressFinalize(this);
    }

    private void GoToUser(object parameter)
    {
        _navigationStore.CurrentViewModel = new ProfileViewModel(Receiver);
    }

    private async void SendMessage(object parameter)
    {
        bool textExist = false;
        bool imageExist = false;

        if (string.IsNullOrWhiteSpace(SendingMessage) == false)
        {
            textExist = true;
        }

        if (textExist || imageExist)
        {
            await _chatService.SendMessage(new MessageContent() { Text = SendingMessage}, Receiver.Username);
            SendingMessage = string.Empty;
        }
    }

    private void UpdateReceivedMessages(MessageViewModel message)
    {
        RaisePropertyChanged(nameof(Messages));
    }

    ~ChatViewModel()
    {
        Dispose();
    }
}