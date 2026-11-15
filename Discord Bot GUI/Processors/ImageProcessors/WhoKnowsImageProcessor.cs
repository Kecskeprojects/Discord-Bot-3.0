using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Tools;
using Discord_Bot.Tools.Extensions;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Discord_Bot.Processors.ImageProcessors;

public class WhoKnowsImageProcessor(BotLogger logger)
{
    private readonly BotLogger logger = logger;

    //Our image size will be 800*500 in the end
    public EditPictureResult EditPicture(Stream originalImg, Dictionary<string, int> plays, string HeadText)
    {
        try
        {
            //Get two instances of the same picture
            using MagickImage mainImage = new(originalImg);
            using IMagickImage<ushort> albumImage = mainImage.Clone();

            ImageTools.CorrectImageRatio(mainImage);

            //Get the dominant and contrast colors for the album image
            MagickColor dominantColor = ImageTools.GetDominantColor(mainImage);
            MagickColor contrastColor = ImageTools.GetContrastColor(dominantColor);

            ResizeImages(mainImage, albumImage);

            //Single color rectangles are drawn half see through
            MagickColor backgroundColor = WithOpacity(contrastColor, 0.5);

            //Rectangle behind album picture
            FillRectangle(mainImage, backgroundColor, 75, 125, 350, 350);

            //Rectangle behind users, starting slightly lower and a little shorter+
            FillRectangle(mainImage, backgroundColor, 425, 132, 300, 333);

            //Place Album image, 1 pixel lower and to the right
            mainImage.Composite(albumImage, 76, 126, CompositeOperator.Over);

            //Get what color should be used for head text
            WriteHeader(HeadText, mainImage, new TextFont("Bot Font", 40), ImageTools.BlackOrWhite(dominantColor));

            //The rectangles behind the users are less see through, the font is smaller,
            //and the leaderboard's text color depends on the contrast color
            WriteUserNames(plays, mainImage, WithOpacity(contrastColor, 0.8), new TextFont("Bot Font", 14), ImageTools.BlackOrWhite(contrastColor));

            string fileName = $"{new Random().Next(0, int.MaxValue)}.png";

            MemoryStream result = new();
            mainImage.Write(result, MagickFormat.Png);

            return new EditPictureResult(fileName, result);
        }
        catch (Exception ex)
        {
            logger.Error("WhoKnowsImageProcessor.cs EditPicture", ex);
        }

        return null;
    }

    #region Helper Methods
    private static void ResizeImages(MagickImage mainImage, IMagickImage<ushort> albumImage)
    {
        //Resize the album picture that is not blurred, 1 pixel less on each side so it can have a proper border
        albumImage.ResizeExact(348, 348);

        //Resize the blurred image, cut it to size, cutting off 150pixels from the top and the bottom
        //Then lastly do a heavy blur on it
        mainImage.ResizeExact(800, 800);
        mainImage.CropTo(0, 150, 800, 500);
        mainImage.Blur(0, 15);
    }

    private static MagickColor WithOpacity(MagickColor color, double opacity)
    {
        return new MagickColor(color.R, color.G, color.B, (ushort) (Quantum.Max * opacity));
    }

    private static void FillRectangle(MagickImage image, MagickColor color, int x, int y, uint width, uint height)
    {
        using MagickImage rectangle = new(color, width, height);
        image.Composite(rectangle, x, y, CompositeOperator.Over);
    }

    private static void WriteHeader(string HeadText, MagickImage mainImage, TextFont font, MagickColor textColor)
    {
        //We write the artist and the song in separate lines in case it turns out too long
        string[] HeadTextparts = HeadText.Replace(" by ", "\nby ").Split("\n");
        for (int i = 0; i < HeadTextparts.Length; i++)
        {
            //Measure the length of the text so we can put it in the middle
            ITypeMetric textsize = MagickTools.MeasureText(HeadTextparts[i], font);

            int X = ((int) mainImage.Width - (int) textsize.TextWidth) / 2;
            int Y = ((125 - ((int) textsize.TextHeight * HeadTextparts.Length)) / 2) + ((int) textsize.TextHeight * i);

            //Put Top text on image
            mainImage.DrawText(HeadTextparts[i], font, textColor, X, Y);
        }
    }

    private static void WriteUserNames(Dictionary<string, int> plays, MagickImage mainImage, MagickColor rowColor, TextFont font, MagickColor textColor)
    {
        //At most 12 users fit on the image
        KeyValuePair<string, int>[] rows = [.. plays.Take(12)];
        for (int i = 0; i < rows.Length; i++)
        {
            string user = $"{i + 1}# {rows[i].Key}";

            //Place slightly less see through rectangle behind text
            FillRectangle(mainImage, rowColor, 425, 132 + (i * 28), 300, 25);

            //Check the length of the user string
            double textWidth = MagickTools.MeasureText(string.Format("{0, 12}", user), font).TextWidth;
            user = ImageTools.ShortenText(font, user, textWidth, 210);

            //Place tranking and name of user
            mainImage.DrawText(user, font, textColor, 427, 137 + (i * 28));

            //Placeholder text, formatting is permanent though
            string points = string.Format("{0,12}", $"{rows[i].Value} plays");
            textWidth = MagickTools.MeasureText(points, font).TextWidth;

            //Amount of plays the user has, right aligned to the rectangle
            mainImage.DrawText(points, font, textColor, 722 - (int) textWidth, 137 + (i * 28));
        }
    }
    #endregion
}
