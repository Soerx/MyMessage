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
    private readonly Action<MessageViewModel> _startEditMessage;

    public Message Message
    {
        get => _message;
        set
        {
            _message = value;
            RaisePropertyChanged(nameof(Message));
        }
    }
    
    public ICommand DeleteCommand { get; }
    public ICommand CopyTextCommand { get; }
    public ICommand StartEditMessageCommand { get; }

    public MessageViewModel(Message message, Action<MessageViewModel> startEditMessage)
    {
        Message = message;
        DeleteCommand = new RelayCommand(Delete);
        CopyTextCommand = new RelayCommand(CopyText);
        StartEditMessageCommand = new RelayCommand(StartEditMessage);
        _startEditMessage = startEditMessage;
    }

    private void Delete(object parameter)
    {
        Message.IsDeleted = true;
    }

    private void CopyText(object parameter)
    {
        Clipboard.SetText(Message.Content.Text);
    }

    private void StartEditMessage(object parameter)
    {
        _startEditMessage(this);
    }
}