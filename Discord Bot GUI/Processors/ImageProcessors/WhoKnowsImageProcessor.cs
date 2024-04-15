using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Tools;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Discord_Bot.Processors.ImageProcessors
{
    public class WhoKnowsImageProcessor(Logging logger)
    {
        private readonly Logging logger = logger;

        //Our image size will be 800*500 in the end
        public EditPictureResult EditPicture(Stream originalImg, Dictionary<string, int> plays, string HeadText)
        {
            try
            {
                //Get two instances of the same picture
                using Image mainImage = Image.Load(originalImg);
                using Image AlbumImage = mainImage.CloneAs<Rgba32>();

                ImageTools.CorrectImageRatio(mainImage, AlbumImage);

                //Get the dominant and contrast colors for the album image
                Tuple<Color, Color> Colors = ImageTools.GetContrastAndDominantColors(mainImage.CloneAs<Rgba32>());
                Color DominantColor = Colors.Item1;
                Color ContrastColor = Colors.Item2;

                ResizeImages(mainImage, AlbumImage);

                //Set oppacity of drawn single color rectangles
                DrawingOptions options = new();
                options.GraphicsOptions.BlendPercentage = 0.5F;

                //Rectangle behind album picture
                mainImage.Mutate(x =>
                    x.Fill(options, ContrastColor, new Rectangle(75, 125, 350, 350))
                );

                //Rectangle behind users, starting slightly lower and a little shorter+
                mainImage.Mutate(x =>
                    x.Fill(options, ContrastColor, new Rectangle(425, 132, 300, 333))
                );

                //Place Album image, 1 pixel lower and to the right
                mainImage.Mutate(x => x.DrawImage(AlbumImage, backgroundLocation: new Point(76, 126), 1));

                //Base font for top text
                Font font = SystemFonts.CreateFont("Bot Font", 40, FontStyle.Regular);

                //Get what color should be used for head text
                Color TextColor = ImageTools.BlackOrWhite(DominantColor);

                WriteHeader(HeadText, mainImage, font, TextColor);

                //Modify graphics option to have a higher oppacity
                options.GraphicsOptions.BlendPercentage = 0.8F;

                //Modify font to be smaller and not bold
                font = SystemFonts.CreateFont("Bot Font", 14, FontStyle.Regular);

                //Get what text color should be used for the leaderboard
                TextColor = ImageTools.BlackOrWhite(ContrastColor);

                int length = plays.Count > 12 ? 12 : plays.Count;

                WriteUserNames(plays, mainImage, ContrastColor, options, font, TextColor, length);

                string fileName = $"{new Random().Next(0, int.MaxValue)}.png";

                MemoryStream result = new();
                mainImage.Save(result, new SixLabors.ImageSharp.Formats.Png.PngEncoder()); // Automatic encoder selected based on extension.

                return new EditPictureResult(fileName, result);
            }
            catch (Exception ex)
            {
                logger.Error("WhoKnowsImageProcessor.cs EditPicture", ex.ToString());
            }

            return null;
        }

        #region Helper Methods
        private static void ResizeImages(Image mainImage, Image AlbumImage)
        {
            //Resize the album picture that is not blurred, 1 pixel less on each side so it can have a proper border
            AlbumImage.Mutate(x => x.Resize(348, 348));

            //Resize the blurred image, cut it to size, cutting off 150pixels from the top and the bottom
            //Then lastly do a heavy blur on it
            mainImage.Mutate(x =>
                x.Resize(800, 800)
                .Crop(new Rectangle(0, 150, 800, 500))
                .GaussianBlur(15)
            );
        }

        private static void WriteHeader(string HeadText, Image mainImage, Font font, Color TextColor)
        {
            //We write the artist and the song in separate lines in case it turns out too long
            string[] HeadTextparts = HeadText.Replace(" by ", "\nby ").Split("\n");
            for (int i = 0; i < HeadTextparts.Length; i++)
            {
                //Measure the length of the text so we can put it in the middle
                FontRectangle textsize = TextMeasurer.MeasureBounds(HeadTextparts[i], new TextOptions(font));

                int X = (mainImage.Width - (int)textsize.Width) / 2;
                int Y = ((125 - ((int)textsize.Height * HeadTextparts.Length)) / 2) + ((int)textsize.Height * i);

                //Put Top text on image
                mainImage.Mutate(x =>
                    x.DrawText(HeadTextparts[i], font, TextColor, new Point(X, Y))
                );
            }
        }

        private static void WriteUserNames(Dictionary<string, int> plays, Image mainImage, Color ContrastColor, DrawingOptions options, Font font, Color TextColor, int length)
        {
            for (int i = 0; i < length; i++)
            {
                //Put dictionary values into temporary variables
                string user = $"{i + 1}# {plays.Keys.ToArray()[i]}";
                int playCount = plays[plays.Keys.ToArray()[i]];

                //Place slightly less see through rectangle behind text
                mainImage.Mutate(x =>
                    x.Fill(options, ContrastColor, new Rectangle(425, 132 + (i * 28), 300, 25))
                );

                //Check the length of the user string
                FontRectangle textsize = TextMeasurer.MeasureBounds(string.Format("{0, 12}", user), new TextOptions(font));
                user = ShortenUsername(font, user, textsize);

                //Place tranking and name of user
                mainImage.Mutate(x => x.DrawText(user, font, TextColor, new Point(427, 137 + (i * 28))));

                //Placeholder text, formatting is permanent though
                string points = string.Format("{0,12}", $"{playCount} plays");
                textsize = TextMeasurer.MeasureBounds(string.Format("{0, 12}", points), new TextOptions(font));

                //Amount of plays the user has
                mainImage.Mutate(x =>
                    x.DrawText(points, font, TextColor, new Point(722 - (int)textsize.Width, 137 + (i * 28)))
                );
            }
        }

        private static string ShortenUsername(Font font, string user, FontRectangle textsize)
        {
            //If it is longer than 210 it will collide with the plays
            if (textsize.Width > 210)
            {
                //We check character by character when it is shorter than that limit
                for (int ch = user.Length; ch > 0; ch--)
                {
                    //We check the currently shortened string's length
                    float tempwidth = TextMeasurer.MeasureBounds(string.Format("{0, 12}", user[..ch]), new TextOptions(font)).Width;

                    //When it is short enough, we shorten the original text to this version and move on
                    if (tempwidth < 210)
                    {
                        user = user[..ch];
                        break;
                    }
                }
            }

            return user;
        }
        #endregion
    }
}
