using DatabaseORM.Interface;
using DatabaseORM.Resources;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Properties;
using Discord_Bot.Tools;
using Discord_Bot.Tools.Extensions;
using ImageMagick;
using System.IO;

namespace Discord_Bot.Processors.ImageProcessors;

public class BiasGameImageProcessor(IBotLogger logger)
{
    private readonly IBotLogger logger = logger;

    public static Stream CombineImages(MemoryStream left, MemoryStream right)
    {
        using MagickImage leftImage = new(left.ToArray());
        using MagickImage rightImage = new(right.ToArray());

        //Widen the canvas to 2 image width plus 50px separator, keeping the left image on the left
        leftImage.Extent(280 + 50 + 280, 338, Gravity.West, MagickColors.Transparent);
        leftImage.Composite(rightImage, 330, 0, CompositeOperator.Over);

        MemoryStream result = new();
        leftImage.Write(result, MagickFormat.Png);

        return result;
    }

    public async Task<Stream> CreatePolaroid(IdolGameResource idol)
    {
        logger.Query($"Getting latest idol image:\n{idol.LatestImageUrl}");
        //The game only picks idols that have an image (IdolRepository.GetListForGameAsync)
        string imageUrl = idol.LatestImageUrl ?? throw new InvalidOperationException($"Idol {idol.IdolId} has no image");
        using MemoryStream stream = await WebTools.GetStream(imageUrl);

        return CreatePolaroid(idol, imageUrl, stream);
    }

    private MemoryStream CreatePolaroid(IdolGameResource idol, string imageUrl, Stream idolImageStream)
    {
        using MagickImage polaroidBase = MagickTools.LoadResource(Resource.polaroid_base);

        using MagickImage idolImage = new(idolImageStream);
        logger.Log("Format: " + idolImage.Format);

        if (!imageUrl.StartsWith(Constant.DbKpopBaseUrl, StringComparison.Ordinal))
        {
            ImageTools.CorrectImageRatio(idolImage, acceptableMargin: 0, cutoffTopRatio: 0.1, extraHeight: 0.05);
        }
        else
        {
            ImageTools.CorrectImageRatio(idolImage, acceptableMargin: 0);
        }

        idolImage.ResizeExact(309, 322); //The Size is the empty part of the polaroid_base

        polaroidBase.Composite(idolImage, 20, 20, CompositeOperator.Over); //The polaroid borders are 20px wide

        WriteText(polaroidBase, idol.StageName ?? "", fontSize: 30, posX: 25, posY: 357);

        WriteText(polaroidBase, idol.GroupFullName, fontSize: 20, posX: 25, posY: 392);

        polaroidBase.ResizeExact(280, 338);

        MemoryStream result = new();
        polaroidBase.Write(result, MagickFormat.Png);

        return result;
    }

    private static void WriteText(MagickImage polaroidBase, string text, int fontSize, int posX, int posY)
    {
        TextFont font = new("Comic Sans MS", fontSize, Bold: true, Italic: true);

        double textWidth = MagickTools.MeasureText(text, font).TextWidth;

        //Text may run up to the polaroid's 20px right border
        int maxWidth = (int) polaroidBase.Width - posX - 20;
        string shortenedText = ImageTools.ShortenText(font, text, textWidth, maxWidth);

        polaroidBase.DrawText(shortenedText, font, MagickColors.Black, posX, posY);
    }
}
