using Prism.Mvvm;
using System;

namespace Client.Models;

public class MessageContent : BindableBase
{
    private string _text = null!;

    public int Id { get; set; }

    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? throw new ArgumentNullException(nameof(Text));
            RaisePropertyChanged(nameof(Text));
        }
    }
}
