using Prism.Mvvm;
using System;

namespace Client.Models;

public class Rank : BindableBase
{
    private string _name = null!;
    private int _priority;
    private string? _description;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value ?? throw new ArgumentNullException(nameof(Name));
            RaisePropertyChanged(nameof(Name));
        }
    }

    public int Priority
    {
        get => _priority;
        set
        {
            _priority = value > 0 ? value : 0;
            RaisePropertyChanged(nameof(Priority));
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            _description = value;
            RaisePropertyChanged(nameof(Description));
        }
    }
}
