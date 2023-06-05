using Client.Tools;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Windows;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;
using Client.ViewModels;
using System.Linq;

namespace Client.Models;

public class User : BindableBase, IDisposable
{
    private const string IMAGE_STRING = "Изображение";

    private bool _diposed;
    private string _username = null!;
    private string _firstname = null!;
    private string _lastname = null!;
    private Gender _gender;
    private DateTime _birthdate;
    private ImageModel? _image;
    private string? _status;
    private bool _isOnline;
    private DateTime _lastActivity;
    private ObservableCollection<MessageViewModel> _messages = null!;

    public int Id { get; set; }

    public string Username
    {
        get => _username ??= string.Empty;
        set
        {
            _username = value;
            RaisePropertyChanged(nameof(Username));
        }
    }

    public string Firstname
    {
        get => _firstname ??= string.Empty;
        set
        {
            _firstname = value;
            RaisePropertyChanged(nameof(Firstname));
            RaisePropertyChanged(nameof(Fullname));
        }
    }

    public string Lastname
    {
        get => _lastname ??= string.Empty;
        set
        {
            _lastname = value;
            RaisePropertyChanged(nameof(Lastname));
            RaisePropertyChanged(nameof(Fullname));
        }
    }

    [JsonIgnore]
    public string Fullname => $"{Lastname} {Firstname}";

    public Gender Gender
    {
        get => _gender;
        set
        {
            _gender = value;
            RaisePropertyChanged(nameof(Gender));
            RaisePropertyChanged(nameof(GenderString));
        }
    }

    [JsonIgnore]
    public string GenderString => Gender.Description();

    public DateTime Birthdate
    {
        get => _birthdate;
        set
        {
            _birthdate = value;
            RaisePropertyChanged(nameof(Birthdate));
            RaisePropertyChanged(nameof(Age));
        }
    }

    [JsonIgnore]
    public int Age => (int)((DateTime.Now - Birthdate).TotalDays / 365);

    public ImageModel? Image
    {
        get => _image;
        set
        {
            _image = value;
            RaisePropertyChanged(nameof(Image));
            RaisePropertyChanged(nameof(DefaultImageVisibility));
        }
    }

    [JsonIgnore]
    public Visibility DefaultImageVisibility => Image is null ? Visibility.Visible : Visibility.Collapsed;

    public string? Status
    {
        get => _status ??= string.Empty;
        set
        {
            _status = value;
            RaisePropertyChanged(nameof(Status));
            RaisePropertyChanged(nameof(StatusVisibility));
        }
    }

    [JsonIgnore]
    public Visibility StatusVisibility => string.IsNullOrWhiteSpace(Status) ? Visibility.Collapsed : Visibility.Visible;

    public bool IsOnline
    {
        get => _isOnline;
        set
        {
            _isOnline = value;
            RaisePropertyChanged(nameof(IsOnline));
            RaisePropertyChanged(nameof(DisplayOnline));
        }
    }

    [JsonIgnore]
    public int? DisplayOnline => IsOnline ? 0 : null;

    public DateTime LastActivity
    {
        get => _lastActivity;
        set
        {
            _lastActivity = value;
            RaisePropertyChanged(nameof(LastActivity));
            RaisePropertyChanged(nameof(LastActivityVisibility));
        }
    }

    [JsonIgnore]
    public Visibility LastActivityVisibility => IsOnline ? Visibility.Collapsed : Visibility.Visible;

    [JsonIgnore]
    public ObservableCollection<MessageViewModel> Messages
    {
        get => _messages ??= new();
        set
        {
            _messages = value;
            RaisePropertyChanged(nameof(Messages));
            RaisePropertyChanged(nameof(UnreadMessagesCount));
            RaisePropertyChanged(nameof(DisplayUnreadMessagesCount));
            RaisePropertyChanged(nameof(LastMessage));
            RaisePropertyChanged(nameof(LastImageText));
            RaisePropertyChanged(nameof(LastImageTextVisibility));
            RaisePropertyChanged(nameof(LastMessageSenderFullname));
        }
    }

    [JsonIgnore]
    public int UnreadMessagesCount => Messages.Where(m => m.Message.IsRead is false && m.Message.SenderUsername == Username).Count();

    [JsonIgnore]
    public int? DisplayUnreadMessagesCount => UnreadMessagesCount == 0 ? null : UnreadMessagesCount;

    [JsonIgnore]
    public Message? LastMessage => Messages.Count > 0 ? Messages.Where(m => m.Message.IsDeleted is false).Last().Message : null;

    [JsonIgnore]
    public string? LastImageText => LastMessage?.Content.Images?.Count > 0 ? IMAGE_STRING : null;

    [JsonIgnore]
    public Visibility LastImageTextVisibility => string.IsNullOrWhiteSpace(LastMessage?.Content.Text) ? Visibility.Visible : Visibility.Collapsed;

    [JsonIgnore]
    public string? LastMessageSenderFullname => LastMessage?.SenderUsername == Username ? $"{this}:" : LastMessage?.SenderUsername == App.Instance.CurrentUser.Username ? "Вы:" : null;

    public User()
    {
        Messages.CollectionChanged += UpdateDependendentProperies;
    }

    private void UpdateDependendentProperies(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(Messages));
        RaisePropertyChanged(nameof(UnreadMessagesCount));
        RaisePropertyChanged(nameof(DisplayUnreadMessagesCount));
        RaisePropertyChanged(nameof(LastMessage));
        RaisePropertyChanged(nameof(LastImageText));
        RaisePropertyChanged(nameof(LastImageTextVisibility));
        RaisePropertyChanged(nameof(LastMessageSenderFullname));
    }

    public void Dispose()
    {
        if (_diposed)
            return;

        Messages.CollectionChanged -= UpdateDependendentProperies;
        _diposed = true;
        GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
        return $"{Lastname} {Firstname}";
    }

    ~User()
    {
        Dispose();
    }
}

public enum Gender
{
    [Description("Не указан")]
    Unspecified,
    [Description("Мужской")]
    Male,
    [Description("Женский")]
    Female
}