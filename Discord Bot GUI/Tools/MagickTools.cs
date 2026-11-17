using Discord_Bot.Communication;
using ImageMagick;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;

namespace Discord_Bot.Tools;

public static class MagickTools
{
    //contentType is null when the response had no Content-Type header, then the data is copied as it is
    public static void ConvertIfUnsupported(HttpContent content, string? contentType, MemoryStream imageData)
    {
        using (Stream tempImageData = content.ReadAsStream())
        {
            switch (contentType)
            {
                case "image/avif":
                {
                    using (MagickImage image = new(tempImageData))
                    {
                        image.Format = MagickFormat.Png;
                        image.Write(imageData);
                    }
                    break;
                }
                default:
                {
                    tempImageData.CopyTo(imageData);
                    break;
                }
            }
        }
    }

    //The embedded images are System.Drawing bitmaps, they go through a PNG stream to become MagickImages
    public static MagickImage LoadResource(Bitmap resource)
    {
        using MemoryStream stream = new();
        resource.Save(stream, ImageFormat.Png);
        stream.Position = 0;

        return new MagickImage(stream);
    }

    public static ITypeMetric MeasureText(string text, TextFont font)
    {
        using MagickImage canvas = new(MagickColors.Transparent, 1, 1);
        font.ApplyTo(canvas.Settings);

        return canvas.FontTypeMetrics(text) ?? throw new InvalidOperationException("Magick.NET could not measure the text");
    }
}
