using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Client.Models;

public class MessageContent : BindableBase
{
    private string? _text;
    private ObservableCollection<ImageModel>? _images;

    public int Id { get; set; }

    public string? Text
    {
        get => _text ??= string.Empty;
        set
        {
            _text = value;
            RaisePropertyChanged(nameof(Text));
        }
    }

    public ObservableCollection<ImageModel>? Images
    {
        get => _images;
        set
        {
            _images = value;
            RaisePropertyChanged(nameof(Images));
        }
    }
}
