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

    public int Id { get; set; }
    public string SenderUsername { get; set; } = null!;

    [JsonIgnore]
    public HorizontalAlignment MessageAlignment => SenderUsername == App.Instance.CurrentUser.Username ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    [JsonIgnore]
    public Visibility PopupVisibility => SenderUsername == App.Instance.CurrentUser.Username && IsDeleted == false ? Visibility.Visible : Visibility.Collapsed;

    public string ReceiverUsername { get; set; } = null!;
    public MessageContent Content { get; set; } = null!;
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

    [JsonIgnore]
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


    [JsonIgnore]
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
            }
        }
    }


    [JsonIgnore]
    public Visibility ReadMarkerVisibility => IsRead && App.Instance.CurrentUser.Username == SenderUsername ? Visibility.Visible : Visibility.Collapsed;

    [JsonIgnore]
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
            }
        }
    }


    [JsonIgnore]
    public Visibility MessageVisibility => IsDeleted ? Visibility.Collapsed : Visibility.Visible;

    [JsonIgnore]
    public Visibility MessageDeletedWarnVisibility => IsDeleted ? Visibility.Visible : Visibility.Collapsed;
}