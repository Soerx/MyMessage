using Client.Commands;
using Client.Models;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels;

public class MessageViewModel : BindableBase
{
    private Message _message = null!;
    private MessageContent _content = null!;
    private Action<MessageViewModel> _messageEditButtonClick;

    public Message Message
    {
        get => _message;
        set
        {
            _message = value;
            RaisePropertyChanged(nameof(Message));
        }
    }

    public MessageContent Content
    {
        get => _content;
        set
        {
            _content = value;
            RaisePropertyChanged(nameof(Content));
        }
    }

    public HorizontalAlignment MessageAlignment => Message.SenderUsername == App.Instance.CurrentUser.Username ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public Visibility PopupVisibility => Message.SenderUsername == App.Instance.CurrentUser.Username && Message.IsDeleted == false ? Visibility.Visible : Visibility.Collapsed;
    
    public ICommand DeleteCommand { get; }
    public ICommand CopyTextCommand { get; }
    public ICommand EditMessageCommand { get; }

    public MessageViewModel(Message message, MessageContent content, Action<MessageViewModel> messageEditButtonClick)
    {
        Message = message;
        Content = content;
        DeleteCommand = new RelayCommand(Delete);
        CopyTextCommand = new RelayCommand(CopyText);
        EditMessageCommand = new RelayCommand(EditMessage);
        _messageEditButtonClick = messageEditButtonClick;
    }

    private void Delete(object parameter)
    {
        Message.IsDeleted = true;
    }

    private void CopyText(object parameter)
    {
        Clipboard.SetText(Content.Text);
    }

    private void EditMessage(object parameter)
    {
        _messageEditButtonClick(this);
    }
}