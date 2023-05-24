using System.IO;
using System.Windows.Media.Imaging;

namespace Client.Tools;

public class ImagesConverter
{
    public static BitmapSource? ByteArrayToImage(byte[]? imageData)
    {
        if (imageData is null)
            return null;

        using var ms = new MemoryStream(imageData);
        var decoder = BitmapDecoder.Create(ms,
            BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return decoder.Frames[0];
    }

    public static byte[] ImageToByteArray(BitmapSource bitmap)
    {
        using MemoryStream ms = new MemoryStream();
        var encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        return ms.GetBuffer();
    }
}