using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Client.Models;

public class MessageContent : BindableBase
{
    private string? _text;
    private ObservableCollection<ImageModel>? _images;
    private const int MAX_DISPLAY_GRID_IMAGES_COLUMNS_COUNT = 3;

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
            RaisePropertyChanged(nameof(DisplayGridImagesColumnsCount));
        }
    }

    [JsonIgnore]
    public int? DisplayGridImagesColumnsCount => Images?.Count > MAX_DISPLAY_GRID_IMAGES_COLUMNS_COUNT ? MAX_DISPLAY_GRID_IMAGES_COLUMNS_COUNT : Images?.Count;
}
