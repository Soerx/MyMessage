using Prism.Mvvm;
using System;

namespace Client.Models;

public class Message : BindableBase
{
    private User _sender = null!;
    private Chat _chat = null!;
    private MessageContent _content = null!;
    private DateTime _created;
    private bool _isEdited;
    private bool _isReceived;
    private bool _isRead;
    private bool _isDeleted;

    public int Id { get; set; }

    public User Sender
    {
        get => _sender;
        set
        {
            _sender = value ?? throw new ArgumentNullException(nameof(Sender));
            RaisePropertyChanged(nameof(Sender));
        }
    }

    public Chat Chat
    {
        get => _chat;
        set
        {
            _chat = value;
            RaisePropertyChanged(nameof(Chat));
        }
    }

    public MessageContent Content
    {
        get => _content;
        set
        {
            _content = value ?? throw new ArgumentNullException(nameof(Content));
            RaisePropertyChanged(nameof(Content));
        }
    }

    public DateTime Created
    {
        get => _created;
        set
        {
            _created = value;
            RaisePropertyChanged(nameof(Created));
        }
    }

    public bool IsEdited
    {
        get => _isEdited;
        set
        {
            _isEdited = value;
            RaisePropertyChanged(nameof(IsEdited));
        }
    }

    public bool IsReceived
    {
        get => _isReceived;
        set
        {
            _isReceived = value;
            RaisePropertyChanged(nameof(IsReceived));
        }
    }

    public bool IsRead
    {
        get => _isRead;
        set
        {
            _isRead = value;
            RaisePropertyChanged(nameof(IsRead));
        }
    }

    public bool IsDeleted
    {
        get => _isDeleted;
        set
        {
            _isDeleted = value;
            RaisePropertyChanged(nameof(IsDeleted));
        }
    }

}
