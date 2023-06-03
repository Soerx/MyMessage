using Client.Tools;
using Prism.Mvvm;
using System;
using System.Text.Json.Serialization;

namespace Client.Models;

public class ImageModel : BindableBase
{
    private string _name = null!;
    private string _path = null!;

    public int Id { get; set; }

    public string Name
    {
        get => _name ??= string.Empty;
        set
        {
            _name = value;
            RaisePropertyChanged(nameof(Name));
        }
    }

    public string Path
    {
        get => _path ??= string.Empty;
        set
        {
            _path = value;
            RaisePropertyChanged(nameof(Path));
            RaisePropertyChanged(nameof(Uri));
        }
    }

    [JsonIgnore]
    public Uri Uri => new($"http://{ServerConnectionTools.IPAddress}:{ServerConnectionTools.PORT}{Path}");
}