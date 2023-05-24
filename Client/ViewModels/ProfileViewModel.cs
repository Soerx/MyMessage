using Client.Models;
using Client.Tools;
using Prism.Mvvm;
using System;
using System.Windows;

namespace Client.ViewModels;

public class ProfileViewModel : BindableBase
{
    public User User { get; }
    public int Age => (int)((DateTime.Now - User.Birthdate).TotalDays / 365);
    public string Gender => User.Gender.Description();
    public Visibility StatusVisibility => string.IsNullOrWhiteSpace(User.Status) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility LastActivityVisibility => User.IsOnline ? Visibility.Collapsed : Visibility.Visible;
    public int? OnlineVisibility => User.IsOnline ? 0 : null;

    public ProfileViewModel(User? user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        User = user;
    }
}