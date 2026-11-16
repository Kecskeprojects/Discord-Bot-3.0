#nullable enable

using Discord_Bot.Communication;
using Discord_Bot.Tools.Extensions;
using ImageMagick;
using System;
using System.Globalization;

namespace Discord_Bot.Tools;

public static class ImageTools
{
    public static void CorrectImageRatio(IMagickImage<ushort> mainImage, double acceptableMargin = 0.2, double cutoffTopRatio = 0.5, double cutoffLeftRatio = 0.5, double extraHeight = 0.0, double extraWidth = 0.0)
    {
        int width = (int) mainImage.Width;
        int height = (int) mainImage.Height;

        //If the Cover image is too wide or high, we crop it depending on which of the two it is
        if (height / (double) width > (1 + acceptableMargin) || height / (double) width < (1 - acceptableMargin))
        {
            int difference = Math.Abs(height - width);
            if (width > height)
            {
                int X = (int) (difference * cutoffLeftRatio);
                int plusWidth = (int) ((width - difference) * extraWidth);
                mainImage.CropTo(X, 0, (uint) (width - difference + plusWidth), (uint) height);
            }
            else
            {
                int Y = (int) (difference * cutoffTopRatio);
                int plusHeight = (int) ((height - difference) * extraHeight);
                mainImage.CropTo(0, Y, (uint) width, (uint) (height - difference + plusHeight));
            }
        }
    }

    public static MagickColor GetDominantColor(IMagickImage<ushort> image)
    {
        //Use nearest neighbor sampling to simplify image and resize it to a 100 pixel wide image with near equal aspect ratio
        using IMagickImage<ushort> sample = image.Clone();
        sample.Sample(new MagickGeometry(100, 0));

        using IPixelCollection<ushort> pixels = sample.GetPixels();
        byte[] rgb = pixels.ToByteArray(PixelMapping.RGB) ?? throw new InvalidOperationException("Magick.NET could not read the pixels of the image");

        //Add up every channel, then put the numbers into the 255 range by dividing it with the total pixels checked
        long r = 0, g = 0, b = 0;
        for (int i = 0; i < rgb.Length; i += 3)
        {
            r += rgb[i];
            g += rgb[i + 1];
            b += rgb[i + 2];
        }
        int totalPixels = rgb.Length / 3;

        return MagickColor.FromRgb((byte) (r / totalPixels), (byte) (g / totalPixels), (byte) (b / totalPixels));
    }

    public static MagickColor GetContrastColor(IMagickColor<ushort> color)
    {
        //By subtracting the values from the maximum we get the exact opposite of the color, making it the most visible color
        return new MagickColor((ushort) (Quantum.Max - color.R), (ushort) (Quantum.Max - color.G), (ushort) (Quantum.Max - color.B));
    }

    public static MagickColor BlackOrWhite(IMagickColor<ushort> color)
    {
        //Formula deciding whether black or white would look more visible, the channels are 16 bit, so the middle is half of Quantum.Max
        double l = (0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B);

        return l < Quantum.Max / 2.0 ? MagickColors.White : MagickColors.Black;
    }

    public static string ShortenText(TextFont font, string text, double textWidth, int maxWidth)
    {
        //If it is longer than the limit it would collide with whatever is next to it
        if (textWidth > maxWidth)
        {
            //We check character by character when it is shorter than that limit
            for (int ch = text.Length; ch > 0; ch--)
            {
                //We check the currently shortened string's length
                double tempwidth = MagickTools.MeasureText(string.Format(CultureInfo.InvariantCulture, "{0, 12}", text[..ch]), font).TextWidth;

                //When it is short enough, we shorten the original text to this version and move on
                if (tempwidth < maxWidth)
                {
                    text = text[..ch];
                    break;
                }
            }
        }

        return text;
    }
}
