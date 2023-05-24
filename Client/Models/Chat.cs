using Prism.Mvvm;
using System;

namespace Client.Models;

public class Chat : BindableBase
{
    private User _owner = null!;
    private string? _name;

    public int Id { get; set; }

    public User Owner
    {
        get => _owner;
        set
        {
            _owner = value ?? throw new ArgumentNullException(nameof(Owner));
            RaisePropertyChanged(nameof(Owner));
        }
    }

    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            RaisePropertyChanged(nameof(Name));
        }
    }


}
