using Client.Commands;
using Client.Controls;
using Client.Models;
using Client.Services;
using Client.Stores;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels;

public class ChatViewModel : BindableBase, IDisposable
{
    private readonly NavigationStore _navigationStore;
    private readonly ChatService _chatService;
    private string? _sendingMessage;
    private bool _disposed;
    private bool _isEditingMessage;
    private MessageViewModel _editingMessage = null!;

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

    public bool IsEditingMessage
    {
        get => _isEditingMessage;
        set
        {
            _isEditingMessage = value;
            RaisePropertyChanged(nameof(IsEditingMessage));
            RaisePropertyChanged(nameof(EditButtonVisibility));
            RaisePropertyChanged(nameof(SendButtonVisibility));
        }
    }

    public Visibility EditButtonVisibility => IsEditingMessage ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SendButtonVisibility => IsEditingMessage ? Visibility.Collapsed : Visibility.Visible;

    public ICommand SendMessageCommand { get; }
    public ICommand EditMessageCommand { get; }
    public ICommand CancelEditMessageCommand { get; }
    public ICommand GoProfileCommand { get; }

    public ChatViewModel(NavigationStore navigationStore, ChatService chatService, User receiver)
    {
        _navigationStore = navigationStore;
        _chatService = chatService;
        Receiver = receiver;
        SendMessageCommand = new RelayCommand(SendMessage);
        GoProfileCommand = new RelayCommand(GoToUser);
        EditMessageCommand = new RelayCommand(EditMessage);
        CancelEditMessageCommand = new RelayCommand(CancelEditMessage);
        _chatService.MessageReceived += UpdateReceivedMessages;
        _chatService.MessageEditButtonClicked += MessageEditButtonClick;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _chatService.MessageReceived -= UpdateReceivedMessages;
        _chatService.MessageEditButtonClicked -= MessageEditButtonClick;
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

    private async void EditMessage(object parameter)
    {
        bool textExist = false;
        bool imageExist = false;

        if (IsEditingMessage == false || _editingMessage is null)
            return;

        if (string.IsNullOrWhiteSpace(SendingMessage) == false && SendingMessage != _editingMessage.Content.Text)
        {
            textExist = true;
        }

        if (textExist || imageExist)
        {
            _editingMessage.Content.Text = SendingMessage;
            _editingMessage.Message.IsEdited = true;
            await _chatService.UpdateMessage(_editingMessage.Message, _editingMessage.Content);
        }

        SendingMessage = string.Empty;
        IsEditingMessage = false;
    }

    private void CancelEditMessage(object parameter)
    {
        SendingMessage = string.Empty;
        IsEditingMessage = false;
    }

    public void MessageEditButtonClick(MessageViewModel sender)
    {
        if (sender.Message.SenderUsername != App.Instance.CurrentUser.Username)
            return;

        _editingMessage = sender;
        SendingMessage = sender.Content.Text;
        IsEditingMessage = true;
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