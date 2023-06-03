using Client.Commands;
using Client.Models;
using Client.Stores;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels;

public class ProfileViewModel : BindableBase
{
    private NavigationStore? _navigationStore;
    private BindableBase? _previewViewModel;

    public User User { get; }

    public ICommand? GoBackCommand { get; }
    public Visibility GoBackButtonVisibility => GoBackCommand is null ? Visibility.Collapsed : Visibility.Visible;

    public ProfileViewModel(User user)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
    }

    public ProfileViewModel(User user, NavigationStore navigationStore, BindableBase previewViewModel) : this(user)
    {
        _navigationStore = navigationStore;
        _previewViewModel = previewViewModel;
        GoBackCommand = new RelayCommand(GoBack);
    }

    private void GoBack(object parameter)
    {
        _navigationStore!.CurrentViewModel = _previewViewModel;
    }
}