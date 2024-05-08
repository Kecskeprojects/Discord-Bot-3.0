using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using Discord_Bot.Tools.Extensions;
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
    public class BiasGameWinnerBracketImageProcessor(Logging logger)
    {
        private readonly Logging logger = logger;

        public MemoryStream UpdateWinnerBracket(BiasGameData biasGameData)
        {
            try
            {
                int[] idolIds = biasGameData.Pairs[biasGameData.CurrentPair];
                List<MemoryStream> files = [
                    (MemoryStream)biasGameData.IdolWithImage[idolIds[0]].Stream,
                    (MemoryStream)biasGameData.IdolWithImage[idolIds[1]].Stream
                ];
                return EditImage(biasGameData, files);
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameWinnerBracketImageProcessor.cs UpdateWinnerBracket", ex);
            }

            return null;
        }

        public MemoryStream AddFinal(BiasGameData data, IdolGameResource winner)
        {
            try
            {
                List<MemoryStream> files = [
                    (MemoryStream)data.IdolWithImage[data.IdolWithImage.Keys.First()].Stream
                ];
                return EditImage(data, files, winner);
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameWinnerBracketImageProcessor.cs AddFinal", ex);
            }

            return null;
        }

        private static MemoryStream EditImage(BiasGameData biasGameData, List<MemoryStream> files, IdolGameResource winner = null)
        {
            StaticRoundData roundData = StaticLists.BiasGameStaticRoundData[biasGameData.CurrentRound - 1];
            using Image winnerBracket = Image.Load<Rgba32>(biasGameData.WinnerBracket.ToArray());

            Image idolImage_1 = CreateImageForRanking(files[0], roundData);
            int position_1 = biasGameData.CurrentPair * 2;
            winnerBracket.Mutate(x => x.DrawImage(idolImage_1, backgroundLocation: new Point(roundData.CalculateX(position_1), roundData.CalculateY(position_1)), 1));

            if (files.Count != 1)
            {
                using Image idolImage_2 = CreateImageForRanking(files[1], roundData);
                int position_2 = (biasGameData.CurrentPair * 2) + 1;
                winnerBracket.Mutate(x => x.DrawImage(idolImage_2, backgroundLocation: new Point(roundData.CalculateX(position_2), roundData.CalculateY(position_2)), 1));
            }
            else
            {
                int posCenterX = roundData.BaseLeftX + (roundData.BaseDiagonal / 2);

                int posY_1 = roundData.BaseY + roundData.BaseDiagonal + 10;
                WriteText(winnerBracket, "WINNER", 40, posCenterX, posY_1, new Color(new Rgba32(30, 140, 255)));

                int posY_2 = roundData.BaseY + roundData.BaseDiagonal + 10 + 40 + 10;
                WriteText(winnerBracket, winner.StageName, 30, posCenterX, posY_2, Color.White);

                int posY_3 = roundData.BaseY + roundData.BaseDiagonal + 10 + 40 + 10 + 30 + 10;
                WriteText(winnerBracket, winner.GroupFullName, 30, posCenterX, posY_3, Color.White);
            }

            MemoryStream result = new();
            winnerBracket.Save(result, new SixLabors.ImageSharp.Formats.Png.PngEncoder());

            return result;
        }

        private static Image<Rgba32> CreateImageForRanking(MemoryStream file, StaticRoundData roundData)
        {
            Image<Rgba32> idolImage = Image.Load<Rgba32>(file.ToArray());

            //The polaroids were resized, so 20 became 16 (the added numbers are the "paddings", the inner image became 247 instead of 309 in width
            idolImage.Mutate(x => x.Crop(new Rectangle(new Point(16, 16), new Size(247, 247))));
            idolImage.Mutate(x => x.Resize(roundData.BaseDiagonal, roundData.BaseDiagonal));

            idolImage.Mutate(x => x.ApplyRoundedCorners(roundData.BaseDiagonal / 2));

            return idolImage;
        }

        private static void WriteText(Image bracket, string text, int fontSize, int posCenterX, int posY, Color color)
        {
            Font font = SystemFonts.CreateFont("Comic Sans MS", fontSize, FontStyle.Bold);

            FontRectangle textsize = TextMeasurer.MeasureBounds(text, new TextOptions(font));

            string shortenedText = ImageTools.ShortenText(font, text, textsize, 400);

            textsize = TextMeasurer.MeasureBounds(shortenedText, new TextOptions(font));

            bracket.Mutate(x =>
                x.DrawText(text, font, color, new Point(posCenterX - ((int)textsize.Width / 2), posY))
            );
        }
    }
}
