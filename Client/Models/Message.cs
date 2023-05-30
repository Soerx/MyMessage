using Prism.Mvvm;
using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace Client.Models;

public class Message : BindableBase
{
    private bool _isEdited;
    private bool _isReceived;
    private bool _isRead;
    private bool _isDeleted;
    private Action<Message> _notifyMessageUpdated = null!;

    public int Id { get; set; }
    public string SenderUsername { get; set; } = null!;
    public string ReceiverUsername { get; set; } = null!;
    public int ContentId { get; set; }
    public DateTime Created { get; set; }

    public bool IsEdited
    {
        get => _isEdited;
        set
        {
            if (IsEdited is false && value != IsEdited)
            {
                _isEdited = value;
                RaisePropertyChanged(nameof(IsEdited));
                RaisePropertyChanged(nameof(EditedMarkerVisibility));
            }
        }
    }

    public Visibility EditedMarkerVisibility => IsEdited ? Visibility.Visible : Visibility.Collapsed;

    public bool IsReceived
    {
        get => _isReceived;
        set
        {
            if (IsReceived is false && value != IsReceived)
            {
                _isReceived = value;
                RaisePropertyChanged(nameof(IsReceived));
                RaisePropertyChanged(nameof(ReceivedMarkerVisibility));
            }
        }
    }

    public Visibility ReceivedMarkerVisibility => IsReceived && App.Instance.CurrentUser.Username == SenderUsername ? Visibility.Visible : Visibility.Collapsed;

    public bool IsRead
    {
        get => _isRead;
        set
        {
            if (IsRead is false && value != IsRead)
            {
                _isRead = true;
                RaisePropertyChanged(nameof(IsRead));
                RaisePropertyChanged(nameof(ReadMarkerVisibility));
                RaisePropertyChanged(nameof(DisplayNotReadMarker));
                _notifyMessageUpdated?.Invoke(this);
            }
        }
    }

    public Visibility ReadMarkerVisibility => IsRead && App.Instance.CurrentUser.Username == SenderUsername ? Visibility.Visible : Visibility.Collapsed;
    public int? DisplayNotReadMarker => IsRead || App.Instance.CurrentUser.Username == ReceiverUsername || IsDeleted ? null : 0;

    public bool IsDeleted
    {
        get => _isDeleted;
        set
        {
            if (IsDeleted is false && value != IsDeleted)
            {
                _isDeleted = value;
                RaisePropertyChanged(nameof(IsDeleted));
                RaisePropertyChanged(nameof(MessageVisibility));
                RaisePropertyChanged(nameof(MessageDeletedWarnVisibility));
                _notifyMessageUpdated?.Invoke(this);
            }
        }
    }

    public Visibility MessageVisibility => IsDeleted ? Visibility.Collapsed : Visibility.Visible;
    public Visibility MessageDeletedWarnVisibility => IsDeleted ? Visibility.Visible : Visibility.Collapsed;

    [JsonConstructor]
    public Message(int id, string senderUsername, string receiverUsername, int contentId, DateTime created, bool isEdited, bool isReceived, bool isRead, bool isDeleted)
    {
        Id = id;
        SenderUsername = senderUsername;
        ReceiverUsername = receiverUsername;
        ContentId = contentId;
        Created = created;
        _isEdited = isEdited;
        _isReceived = isReceived;
        _isRead = isRead;
        _isDeleted = isDeleted;
    }

    public Message(Message message, Action<Message> readMessage)
    {
        Id = message.Id;
        SenderUsername = message.SenderUsername;
        ReceiverUsername = message.ReceiverUsername;
        ContentId = message.ContentId;
        Created = message.Created;
        _isEdited = message.IsEdited;
        _isReceived = message.IsReceived;
        _isRead = message.IsRead;
        _isDeleted = message.IsDeleted;
        _notifyMessageUpdated = readMessage;
    }
}