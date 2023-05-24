﻿using Client.Args;
using Client.Commands;
using Client.Services;
using Client.Stores;
using MahApps.Metro.Controls;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels;

public class AuthViewModel : BindableBase, IDisposable
{
    private readonly NavigationStore _navigationStore;
    private bool _disposed;
    private string _username;
    private string _password;
    private PasswordBox? _passwordBox;
    private ToggleSwitch? _passwordVisibilitySwitcher;
    private Visibility _passwordVisibility;
    private Visibility _messageVisibility;
    private string? _message;

    public string Username
    {
        get => _username;
        set
        {
            _username = value ?? string.Empty;
            RaisePropertyChanged(nameof(Username));
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

    public PasswordBox? PasswordField
    {
        get => _passwordBox;
        private set
        {
            _passwordBox = value;
            RaisePropertyChanged(nameof(PasswordField));
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

    public string? Message
    {
        get => _message;
        private set
        {
            _message = value;
            RaisePropertyChanged(nameof(Message));
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

    public ICommand AuthCommand { get; }
    public ICommand GoRegisterCommand { get; }

    public AuthViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _username = string.Empty;
        _password = string.Empty;
        _passwordBox = new PasswordBox();
        PasswordVisibilitySwitcher = new ToggleSwitch();
        PasswordVisibilitySwitcher.OffContent = "Показать пароль";
        PasswordVisibilitySwitcher.OnContent = "Скрыть пароль";
        PasswordVisibilitySwitcher.Toggled += SwitchPasswordVisibility;
        AuthCommand = new RelayCommand(Auth);
        GoRegisterCommand = new RelayCommand(GoRegister);
        PasswordVisibility = Visibility.Collapsed;
        MessageVisibility = Visibility.Collapsed;
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

    private void Auth(object parameter)
    {
        Message = string.Empty;
        MessageVisibility = Visibility.Collapsed;

        if (PasswordVisibility is Visibility.Collapsed)
            Password = PasswordField?.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            return;

        using AuthService db = new();
        
        if (db.Auth(new AuthArgs(Username, Password), out string? token, out string? exception) && token is not null)
        {
            _navigationStore.CurrentViewModel = new HomeViewModel(_navigationStore, token);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(exception) == false)
            {
                Message += $"• {exception}";
            }
            else
            {
                Message += "• Неверный логин или пароль";
            }

            MessageVisibility = Visibility.Visible;
        }
    }

    private void GoRegister(object parameter)
    {
        _navigationStore.CurrentViewModel = new RegisterViewModel(_navigationStore);
        Dispose(false);
    }

    private void SwitchPasswordVisibility(object sender, RoutedEventArgs e)
    {
        if (PasswordField is null)
            return;

        if (PasswordVisibility is Visibility.Collapsed)
        {
            Password = PasswordField.Password;
            PasswordVisibility = Visibility.Visible;
            PasswordField.Visibility = Visibility.Collapsed;
        }
        else
        {
            PasswordField.Password = Password;
            PasswordField.Visibility = Visibility.Visible;
            PasswordVisibility = Visibility.Collapsed;
        }
    }

    ~AuthViewModel()
    {
        Dispose(false);
    }
}