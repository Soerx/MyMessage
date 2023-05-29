using Client.Commands;
using Client.Stores;
using MahApps.Metro.Controls;
using Prism.Mvvm;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System;
using Client.Services;
using Client.Args;
using Client.Models;
using Client.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.ViewModels;

public class RegisterViewModel : BindableBase, IDisposable
{
    private readonly NavigationStore _navigationStore;
    private bool _disposed;
    private string _username;
    private string _firstname;
    private string _lastname;
    private List<GenderWrapper> _gendersWrappers;
    private GenderWrapper _genderWrapper = null!;
    private DateTime _birthdate;
    private string _password;
    private string _repeatPassword;
    private PasswordBox? _passwordBox;
    private PasswordBox? _repeatPasswordBox;
    private ToggleSwitch? _passwordVisibilitySwitcher;
    private Visibility _passwordVisibility;
    private Visibility _messageVisibility;
    private string? _message;
    private bool _isAvailable;

    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            _isAvailable = value;
            RaisePropertyChanged(nameof(IsAvailable));
            RaisePropertyChanged(nameof(ProgressRingVisibility));
        }
    }

    public Visibility ProgressRingVisibility => IsAvailable ? Visibility.Collapsed : Visibility.Visible;

    public string Username
    {
        get => _username;
        set
        {
            _username = value ?? string.Empty;
            RaisePropertyChanged(nameof(Username));
        }
    }

    public string Firstname
    {
        get => _firstname;
        set
        {
            _firstname = value ?? string.Empty;
            RaisePropertyChanged(nameof(Firstname));
        }
    }

    public string Lastname
    {
        get => _lastname;
        set
        {
            _lastname = value ?? string.Empty;
            RaisePropertyChanged(nameof(Lastname));
        }
    }

    public List<GenderWrapper> GendersWrappers
    {
        get => _gendersWrappers;
        set
        {
            _gendersWrappers = value;
            RaisePropertyChanged(nameof(GendersWrappers));
        }
    }

    public GenderWrapper SelectedGenderWrapper
    {
        get => _genderWrapper;
        set
        {
            _genderWrapper = value;
            RaisePropertyChanged(nameof(SelectedGenderWrapper));
        }
    }

    public DateTime Birthdate
    {
        get => _birthdate;
        set
        {
            _birthdate = value;
            RaisePropertyChanged(nameof(Birthdate));
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value ?? string.Empty;
            RaisePropertyChanged(nameof(Password));
        }
    }

    public string RepeatPassword
    {
        get => _repeatPassword;
        set
        {
            _repeatPassword = value ?? string.Empty;
            RaisePropertyChanged(nameof(RepeatPassword));
        }
    }

    public PasswordBox? PasswordField
    {
        get => _passwordBox;
        private set
        {
            _passwordBox = value;
            RaisePropertyChanged(nameof(PasswordField));
        }
    }

    public PasswordBox? RepeatPasswordField
    {
        get => _repeatPasswordBox;
        private set
        {
            _repeatPasswordBox = value;
            RaisePropertyChanged(nameof(RepeatPasswordField));
        }
    }

    public Visibility PasswordVisibility
    {
        get => _passwordVisibility;
        set
        {
            _passwordVisibility = value;
            RaisePropertyChanged(nameof(PasswordVisibility));
        }
    }

    public Visibility MessageVisibility
    {
        get => _messageVisibility;
        set
        {
            _messageVisibility = value;
            RaisePropertyChanged(nameof(MessageVisibility));
        }
    }

    public ToggleSwitch? PasswordVisibilitySwitcher
    {
        get => _passwordVisibilitySwitcher;
        private set
        {
            _passwordVisibilitySwitcher = value;
            RaisePropertyChanged(nameof(PasswordVisibilitySwitcher));
        }
    }

    public string? Message
    {
        get => _message;
        private set
        {
            _message = value;
            RaisePropertyChanged(nameof(Message));
        }
    }

    public ICommand RegisterCommand { get; }
    public ICommand GoAuthCommand { get; }

    public RegisterViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _username = string.Empty;
        _firstname = string.Empty;
        _lastname = string.Empty;
        _password = string.Empty;
        _repeatPassword = string.Empty;
        _passwordBox = new PasswordBox();
        _repeatPasswordBox = new PasswordBox();
        PasswordVisibilitySwitcher = new ToggleSwitch();
        PasswordVisibilitySwitcher.OffContent = "Показать пароль";
        PasswordVisibilitySwitcher.OnContent = "Скрыть пароль";
        PasswordVisibilitySwitcher.Toggled += SwitchPasswordVisibility;
        RegisterCommand = new AsyncRelayCommand(Register);
        GoAuthCommand = new RelayCommand(GoAuth);
        PasswordVisibility = Visibility.Collapsed;
        MessageVisibility = Visibility.Collapsed;
        int minUserAge = 14;
        Birthdate = DateTime.Now.AddYears(-minUserAge);
        _gendersWrappers = new List<GenderWrapper>();

        foreach (Gender gender in Enum.GetValues(typeof(Gender)))
        {
            _gendersWrappers.Add(new GenderWrapper(gender));
        }

        SelectedGenderWrapper = new GenderWrapper(Gender.Unspecified);
        IsAvailable = true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _username = string.Empty;
        }

        if (PasswordVisibilitySwitcher is not null)
            PasswordVisibilitySwitcher.Toggled -= SwitchPasswordVisibility;

        PasswordField = null;
        PasswordVisibilitySwitcher = null;
        _disposed = true;
    }

    private async Task Register(object parameter)
    {
        IsAvailable = false;
        Message = string.Empty;
        MessageVisibility = Visibility.Collapsed;

        if (CheckFieldsValuesCorrect() == false)
            return;

        using AuthService authService = new();
        var result = await authService.Register(new RegisterArgs(Username,
                                                  Password,
                                                  Firstname,
                                                  Lastname,
                                                  SelectedGenderWrapper.Gender,
                                                  Birthdate));
        bool isRegisterSuccess = result.Item1;

        if (isRegisterSuccess)
        {
            string token = result.Item2!;
            _navigationStore.CurrentViewModel = new HomeViewModel(_navigationStore, token);
        }
        else
        {
            string errorMessage = result.Item3!;
            Message += $"• {errorMessage}";
            MessageVisibility = Visibility.Visible;
        }

        IsAvailable = true;
    }

    private bool CheckFieldsValuesCorrect()
    {
        if (PasswordVisibility is Visibility.Collapsed)
        {
            Password = PasswordField?.Password ?? string.Empty;
            RepeatPassword = RepeatPasswordField?.Password ?? string.Empty;
        }

        if (CheckFieldValueLengthCorrect(fieldName: "Логин", Username, minLength: 4, maxLength: 20) &&
        CheckFieldValueLengthCorrect(fieldName: "Пароль", Password, minLength: 8, maxLength: 80) &&
        CheckFieldValueLengthCorrect(fieldName: "Имя", Firstname, minLength: 2, maxLength: 120) &&
        CheckFieldValueLengthCorrect(fieldName: "Фамилия", Lastname, minLength: 2, maxLength: 120))
        {
            if (Password == RepeatPassword == false)
            {
                Message = "• Пароли не совпадают.";
                MessageVisibility = Visibility.Visible;
                return false;
            }

            string minBirthdateDate = "01.01.1900";
            int minUserAge = 14;

            if (Birthdate.Date > DateTime.Parse(minBirthdateDate) &&
                Birthdate.Date < DateTime.Now.AddYears(-minUserAge))
            {
                return true;
            }
            else
            {
                Message += $"\n• Некорректная дата рождения (Вы должны быть старше {minUserAge})";
            }
        }

        MessageVisibility = Visibility.Visible;
        return false;
    }

    private bool CheckFieldValueLengthCorrect(string fieldName, string fieldValue, int minLength, int maxLength)
    {
        if (fieldValue.Length > maxLength || fieldValue.Length < minLength)
        {
            Message += $"\n• Поле [{fieldName}] должно быть не менее {minLength} и не более {maxLength} символов";
            return false;
        }

        return true;
    }

    private void GoAuth(object parameter)
    {
        _navigationStore.CurrentViewModel = new AuthViewModel(_navigationStore);
        Dispose(false);
    }

    private void SwitchPasswordVisibility(object sender, RoutedEventArgs e)
    {
        if (PasswordField is null || RepeatPasswordField is null)
            return;

        if (PasswordVisibility is Visibility.Collapsed)
        {
            Password = PasswordField.Password;
            RepeatPassword = RepeatPasswordField.Password;
            PasswordField.Visibility = Visibility.Collapsed;
            RepeatPasswordField.Visibility = Visibility.Collapsed;
            PasswordVisibility = Visibility.Visible;
        }
        else
        {
            PasswordField.Password = Password;
            RepeatPasswordField.Password = RepeatPassword;
            PasswordField.Visibility = Visibility.Visible;
            RepeatPasswordField.Visibility = Visibility.Visible;
            PasswordVisibility = Visibility.Collapsed;
        }
    }

    public class GenderWrapper
    {
        public Gender Gender { get; }

        public GenderWrapper(Gender gender)
        {
            Gender = gender;
        }

        public override string ToString()
        {
            return Gender.Description();
        }
    }

    ~RegisterViewModel()
    {
        Dispose(false);
    }
}