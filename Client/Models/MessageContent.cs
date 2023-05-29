using Client.Tools;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Client.Models;

public class MessageContent : BindableBase
{
    private string? _text;
    private List<byte[]>? _images;

    public int Id { get; set; }

    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            RaisePropertyChanged(nameof(Text));
        }
    }

    public List<byte[]>? Images
    {
        get => _images;
        set
        {
            _images = value;
            RaisePropertyChanged(nameof(Images));
            RaisePropertyChanged(nameof(BitmapImages));
        }
    }

    public List<BitmapSource>? BitmapImages
    {
        get
        {
            if (Images is not null)
            {
                List<BitmapSource> images = new();

                foreach (var imageData in Images)
                    images.Add(ImagesConverter.ByteArrayToImage(imageData)!);

                return images;
            }

            return null;
        }
    }
}
