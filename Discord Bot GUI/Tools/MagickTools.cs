using ImageMagick;
using System.IO;
using System.Net.Http;

namespace Discord_Bot.Tools;

public static class MagickTools
{
    public static void ConvertIfUnsupported(HttpContent content, string contentType, MemoryStream imageData)
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
}
