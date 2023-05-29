using Client.Models;
using Client.Tools;
using Prism.Mvvm;
using System;
using System.Windows;

namespace Client.ViewModels;

public class ProfileViewModel : BindableBase
{
    public User User { get; }

    public ProfileViewModel(User? user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        User = user;
    }
}