using Discord_Bot.Core;
using Discord_Bot.Properties;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Processors.ImageProcessors;

public class BiasGameImageProcessor(BotLogger logger)
{
    private readonly BotLogger logger = logger;

    public static Stream CombineImages(MemoryStream left, MemoryStream right)
    {
        using Image leftImage = Image.Load<Rgba32>(left.ToArray());
        using Image rightImage = Image.Load<Rgba32>(right.ToArray());

        leftImage.Mutate(x => x.Resize(new ResizeOptions()
        {
            Mode = ResizeMode.BoxPad,
            Position = AnchorPositionMode.Left,
            Size = new Size(280 + 50 + 280, 338) //2 image width plus 50px separator
        }));
        leftImage.Mutate(x => x.DrawImage(rightImage, backgroundLocation: new Point(330, 0), 1));

        MemoryStream result = new();
        leftImage.Save(result, new SixLabors.ImageSharp.Formats.Png.PngEncoder());

        return result;
    }

    public async Task<Stream> CreatePolaroid(IdolGameResource idol)
    {
        MemoryStream polaroidStream = new();
        Resource.polaroid_base.Save(polaroidStream, System.Drawing.Imaging.ImageFormat.Png);
        using Image polaroidBase = Image.Load<Rgba32>(polaroidStream.ToArray());

        logger.Query($"Getting latest idol image:\n{idol.LatestImageUrl}");
        using Image idolImage = Image.Load<Rgba32>(await WebTools.GetStream(idol.LatestImageUrl));

        if (!idol.LatestImageUrl.StartsWith("https://dbkpop.com"))
        {
            ImageTools.CorrectImageRatio(idolImage, acceptableMargin: 0, cutoffTopRatio: 0.1, extraHeight: 0.05);
        }
        else
        {
            ImageTools.CorrectImageRatio(idolImage, acceptableMargin: 0);
        }

        idolImage.Mutate(x => x.Resize(new Size(309, 322))); //The Size is the empty part of the polaroid_base

        polaroidBase.Mutate(x => x.DrawImage(idolImage, backgroundLocation: new Point(20, 20), 1)); //The polaroid borders are 20px wide

        WriteText(polaroidBase, idol.StageName, fontSize: 30, posX: 25, posY: 357);

        WriteText(polaroidBase, idol.GroupFullName, fontSize: 20, posX: 25, posY: 392);

        polaroidBase.Mutate(x => x.Resize(280, 338));

        MemoryStream result = new();
        polaroidBase.Save(result, new SixLabors.ImageSharp.Formats.Png.PngEncoder());

        return result;
    }

    private static void WriteText(Image polaroidBase, string text, int fontSize, int posX, int posY)
    {
        Font font = SystemFonts.CreateFont("Comic Sans MS", fontSize, FontStyle.BoldItalic);

        FontRectangle textsize = TextMeasurer.MeasureBounds(text, new TextOptions(font));

        string shortenedText = ImageTools.ShortenText(font, text, textsize, 372);

        polaroidBase.Mutate(x =>
            x.DrawText(text, font, Color.Black, new Point(posX, posY))
        );
    }
}
