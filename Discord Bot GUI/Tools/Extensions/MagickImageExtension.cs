using Discord_Bot.Communication;
using ImageMagick;
using ImageMagick.Drawing;

namespace Discord_Bot.Tools.Extensions;

public static class MagickImageExtension
{
    //Stretches the image to exactly this size, Catrom is the bicubic filter ImageSharp used by default
    public static void ResizeExact(this IMagickImage<ushort> image, uint width, uint height)
    {
        image.Resize(new MagickGeometry(width, height) { IgnoreAspectRatio = true }, FilterType.Catrom);
    }

    //ImageMagick keeps the crop offset on the image's canvas after a crop, ResetPage clears it
    public static void CropTo(this IMagickImage<ushort> image, int x, int y, uint width, uint height)
    {
        image.Crop(new MagickGeometry(x, y, width, height));
        image.ResetPage();
    }

    public static void ApplyRoundedCorners(this IMagickImage<ushort> image, double cornerRadius)
    {
        //Draw the shape we want to keep on a transparent mask, then keep only the parts of the image under it
        using MagickImage mask = new(MagickColors.Transparent, image.Width, image.Height);
        _ = new Drawables()
            .FillColor(MagickColors.White)
            .RoundRectangle(0, 0, image.Width - 1, image.Height - 1, cornerRadius, cornerRadius)
            .Draw(mask);

        image.Alpha(AlphaAction.Set);
        image.Composite(mask, CompositeOperator.DstIn);
    }

    //Draws text with its top left corner at (x, y), ImageMagick places text by its baseline, so the font's ascent is added
    public static void DrawText(this IMagickImage<ushort> image, string text, TextFont font, IMagickColor<ushort> color, int x, int y)
    {
        double ascent = MagickTools.MeasureText(text, font).Ascent;

        _ = font.CreateDrawables()
            .FillColor(color)
            .Text(x, y + ascent, text)
            .Draw(image);
    }
}
