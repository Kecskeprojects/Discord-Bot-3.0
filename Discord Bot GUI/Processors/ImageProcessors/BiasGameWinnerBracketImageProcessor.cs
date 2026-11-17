using DatabaseORM.Interface;
using DatabaseORM.Resources;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Tools;
using Discord_Bot.Tools.Extensions;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Discord_Bot.Processors.ImageProcessors;

public class BiasGameWinnerBracketImageProcessor(IBotLogger logger)
{
    private readonly IBotLogger logger = logger;

    //Null when the image could not be made, the error is logged
    public MemoryStream? UpdateWinnerBracket(BiasGameData biasGameData)
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

    //Null when the image could not be made, the error is logged
    public MemoryStream? AddFinal(BiasGameData data, IdolGameResource winner)
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

    private static MemoryStream EditImage(BiasGameData biasGameData, List<MemoryStream> files, IdolGameResource? winner = null)
    {
        StaticRoundData roundData = Constant.BiasGameStaticRoundData[biasGameData.CurrentRound - 1];
        using MagickImage winnerBracket = new(biasGameData.WinnerBracket.ToArray());

        using MagickImage idolImage_1 = CreateImageForRanking(files[0], roundData);
        int position_1 = biasGameData.CurrentPair * 2;
        winnerBracket.Composite(idolImage_1, roundData.CalculateX(position_1), roundData.CalculateY(position_1), CompositeOperator.Over);

        //UpdateWinnerBracket passes the two images of the current pair, AddFinal passes the winner's image and data
        if (winner == null)
        {
            using MagickImage idolImage_2 = CreateImageForRanking(files[1], roundData);
            int position_2 = (biasGameData.CurrentPair * 2) + 1;
            winnerBracket.Composite(idolImage_2, roundData.CalculateX(position_2), roundData.CalculateY(position_2), CompositeOperator.Over);
        }
        else
        {
            int posCenterX = roundData.BaseLeftX + (roundData.BaseDiagonal / 2);

            int posY_1 = roundData.BaseY + roundData.BaseDiagonal + 10;
            WriteText(winnerBracket, "WINNER", 40, posCenterX, posY_1, MagickColor.FromRgb(30, 140, 255));

            int posY_2 = roundData.BaseY + roundData.BaseDiagonal + 10 + 40 + 10;
            WriteText(winnerBracket, winner.StageName ?? "", 30, posCenterX, posY_2, MagickColors.White);

            int posY_3 = roundData.BaseY + roundData.BaseDiagonal + 10 + 40 + 10 + 30 + 10;
            WriteText(winnerBracket, winner.GroupFullName, 30, posCenterX, posY_3, MagickColors.White);
        }

        MemoryStream result = new();
        winnerBracket.Write(result, MagickFormat.Png);

        return result;
    }

    private static MagickImage CreateImageForRanking(MemoryStream file, StaticRoundData roundData)
    {
        MagickImage idolImage = new(file.ToArray());

        //The polaroids were resized, so 20 became 16 (the added numbers are the "paddings", the inner image became 247 instead of 309 in width
        idolImage.CropTo(16, 16, 247, 247);
        idolImage.ResizeExact((uint) roundData.BaseDiagonal, (uint) roundData.BaseDiagonal);

        idolImage.ApplyRoundedCorners(roundData.BaseDiagonal / 2);

        return idolImage;
    }

    private static void WriteText(MagickImage bracket, string text, int fontSize, int posCenterX, int posY, IMagickColor<ushort> color)
    {
        TextFont font = new("Comic Sans MS", fontSize, Bold: true);

        double textWidth = MagickTools.MeasureText(text, font).TextWidth;

        string shortenedText = ImageTools.ShortenText(font, text, textWidth, 400);

        textWidth = MagickTools.MeasureText(shortenedText, font).TextWidth;

        bracket.DrawText(shortenedText, font, color, posCenterX - ((int) textWidth / 2), posY);
    }
}
