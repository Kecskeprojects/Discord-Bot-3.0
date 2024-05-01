using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Tools;
using SixLabors.ImageSharp;
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
                logger.Error("BiasGameWinnerBracketImageProcessor.cs UpdateWinnerBracket", ex.ToString());
            }

            return null;
        }

        public MemoryStream AddFinal(BiasGameData data)
        {
            try
            {
                Dictionary<int, Discord.FileAttachment>.KeyCollection idolIds = data.IdolWithImage.Keys;
                List<MemoryStream> files = [
                    (MemoryStream)data.IdolWithImage[idolIds.First()].Stream
                ];
                return EditImage(data, files);
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameWinnerBracketImageProcessor.cs AddFinal", ex.ToString());
            }

            return null;
        }

        private static MemoryStream EditImage(BiasGameData biasGameData, List<MemoryStream> files)
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
    }
}
