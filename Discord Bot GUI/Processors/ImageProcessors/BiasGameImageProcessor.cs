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
        //Todo: kpopdb images are wide with the face in the middle, kprofile images are vertical with the face in the top to middle part, this should be accounted for
        public async Task<Stream> CreatePolaroid(IdolGameResource idol)
        {
            try
            {
                MemoryStream polaroidStream = new();
                Resource.polaroid_base.Save(polaroidStream, System.Drawing.Imaging.ImageFormat.Png);
                using Image polaroidBase = Image.Load<Rgba32>(polaroidStream.ToArray());

                using Image idolImage = Image.Load<Rgba32>(await Global.GetStream(idol.LatestImageUrl));

                if (!idol.LatestImageUrl.StartsWith("https://dbkpop.com"))
                {
                    ImageTools.CorrectImageRatio(idolImage, acceptableMargin: 0.05, cutoffTopRatio: 0.1, extraHeight: 0.05);
                }
                else
                {
                    ImageTools.CorrectImageRatio(idolImage);
                }
                
                idolImage.Mutate(x => x.Resize(new Size(309, 322))); //The Size is the empty part of the polaroid_base

                polaroidBase.Mutate(x => x.DrawImage(idolImage, backgroundLocation: new Point(20, 20), 1)); //The polaroid borders are 20px wide

                WriteText(polaroidBase, idol.StageName, fontSize: 30, posX: 25, posY: 357);

                WriteText(polaroidBase, idol.GroupFullName ?? "Soloist", fontSize: 20, posX: 25, posY: 392);

                polaroidBase.Mutate(x => x.Resize(280, 338));

                polaroidBase.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.BoxPad,
                    Position = AnchorPositionMode.Center,
                    Size = new Size(300, 500)
                }));

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
