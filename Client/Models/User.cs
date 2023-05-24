using Client.Tools;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Client.Models;

public class User : BindableBase
{
    private string _username = null!;
    private Rank _rank = null!;
    private string _firstname = null!;
    private string _lastname = null!;
    private Gender _gender;
    private DateTime _birthdate;
    private byte[]? _image;
    private string? _status;
    private bool _isOnline;
    private DateTime _lastActivity;

    public int Id { get; set; }

    public string Username
    {
        get => _username;
        set
        {
            _username = value ?? throw new ArgumentNullException(nameof(Username));
            RaisePropertyChanged(nameof(Username));
        }
    }

    public Rank Rank
    {
        get => _rank;
        set
        {
            _rank = value ?? throw new ArgumentNullException(nameof(Rank));
            RaisePropertyChanged(nameof(Rank));
        }
    }

    public string Firstname
    {
        get => _firstname;
        set
        {
            _firstname = value ?? throw new ArgumentNullException(nameof(Firstname));
            RaisePropertyChanged(nameof(Firstname));
        }
    }

    public string Lastname
    {
        get => _lastname;
        set
        {
            _lastname = value ?? throw new ArgumentNullException(nameof(Lastname));
            RaisePropertyChanged(nameof(Lastname));
        }
    }

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

    public int Age => (int)((DateTime.Now - Birthdate).TotalDays / 365);

    public byte[]? Image
    {
        get => _image;
        set
        {
            _image = value;
            RaisePropertyChanged(nameof(Image));
            RaisePropertyChanged(nameof(BitmapImage));
        }
    }

    public BitmapSource? BitmapImage => ImagesConverter.ByteArrayToImage(Image);

    public string? Status
    {
        get => _status;
        set
        {
            _status = value;
            RaisePropertyChanged(nameof(Status));
            RaisePropertyChanged(nameof(StatusVisibility));
        }
    }

    public Visibility StatusVisibility => string.IsNullOrWhiteSpace(Status) ? Visibility.Collapsed : Visibility.Visible;

    public bool IsOnline
    {
        get => _isOnline;
        set
        {
            _isOnline = value;
            RaisePropertyChanged(nameof(IsOnline));
            RaisePropertyChanged(nameof(OnlineVisibility));
        }
    }

    public int? OnlineVisibility => IsOnline ? 0 : null;

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

    public Visibility LastActivityVisibility => IsOnline ? Visibility.Collapsed : Visibility.Visible;

    public override string ToString()
    {
        return $"{Lastname} {Firstname}";
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
