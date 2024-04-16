using Discord_Bot.Core;
using Discord_Bot.Properties;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Processors.ImageProcessors
{
    public class BiasGameImageProcessor(Logging logger)
    {
        private readonly Logging logger = logger;

        public async Task<Stream> CreatePolaroid(IdolGameResource idol)
        {
            try
            {
                const int idolImageWidth = 309;
                const int idolImageHeight = 322;

                MemoryStream polaroidStream = new();
                Resource.polaroid_base.Save(polaroidStream, System.Drawing.Imaging.ImageFormat.Png);
                using Image polaroidBase = Image.Load<Rgba32>(polaroidStream.ToArray());

                using Image idolImage = Image.Load<Rgba32>(await Global.GetStream(idol.LatestImageUrl));

                ImageTools.CorrectImageRatio(idolImage);
                idolImage.Mutate(x => x.Resize(new Size(idolImageWidth, idolImageHeight)));

                polaroidBase.Mutate(x => x.DrawImage(idolImage, backgroundLocation: new Point(20, 20), 1));

                WriteText(polaroidBase, idol.StageName, 30, 25, 357);

                WriteText(polaroidBase, idol.GroupFullName ?? "Soloist", 20, 25, 392);

                polaroidBase.Mutate(x => x.Resize(280, 338));

                MemoryStream result = new();
                polaroidBase.Save(result, new SixLabors.ImageSharp.Formats.Png.PngEncoder());

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameImageProcessor.cs CreatePolaroid", ex.ToString());
            }

            return null;
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
}
