using System.IO;
using DatabaseORM.Interface;
using DatabaseORM.Resources;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Tests.Common.Constants;
using ImageMagick;

namespace Discord_Bot.Tests.Unit.Bot.Processors.ImageProcessors;

//Smoke tests of the Magick.NET images: each comes out in its format and size from a plain picture made here. The pixels aren't
//compared; the ImageSharp port compared them once (see the port's notes). The text uses the fonts of the machine
//("Bot Font" for the current user, Comic Sans MS), as the bot does
public sealed class ImageProcessorTests
{
    private readonly IBotLogger logger = Substitute.For<IBotLogger>();

    [Fact]
    public void BonkGif_IsATwoFrameLoopingGifWithTheDelay()
    {
        using MemoryStream avatar = Picture(128, 128);

        using MemoryStream? gif = new BonkGifProcessor(logger).CreateBonkImage(avatar, 200);

        Assert.NotNull(gif);
        using MagickImageCollection frames = new(gif.ToArray());
        Assert.Equal(2, frames.Count);
        Assert.All(frames, frame =>
        {
            Assert.Equal(MagickFormat.Gif, frame.Format);
            Assert.Equal((1000u, 1000u), (frame.Width, frame.Height));
            //200 ms, in the hundredths of a second GIF counts in
            Assert.Equal(20u, frame.AnimationDelay);
        });
    }

    [Fact]
    public void BonkGif_PictureThatIsNoImage_GivesNullAndLogsWhy()
    {
        using MemoryStream notAnImage = new([1, 2, 3, 4]);

        Assert.Null(new BonkGifProcessor(logger).CreateBonkImage(notAnImage, 200));
        logger.Received().Error("BonkGifProcessor.cs CreateBonkImage", Arg.Any<Exception>());
    }

    [Fact]
    public void WhoKnowsImage_IsAPngOfTheRankingsSize()
    {
        using MemoryStream cover = Picture(600, 600);

        using EditPictureResult? result = new WhoKnowsImageProcessor(logger).EditPicture(cover, new Dictionary<string, int> { ["Bob"] = 9, ["Alice"] = 3 }, "Shut Down by BLACKPINK");

        Assert.NotNull(result);
        Assert.EndsWith(".png", result.FileName, StringComparison.Ordinal);
        AssertImage(result.Stream, MagickFormat.Png, 800, 500);
    }

    //Only 12 rows fit; the rest are left off the image
    [Fact]
    public void WhoKnowsImage_MoreListenersThanFit_StillComesOut()
    {
        using MemoryStream cover = Picture(300, 500);
        Dictionary<string, int> plays = Enumerable.Range(1, 30).ToDictionary(i => $"A rather long listener name {i}", i => 31 - i);

        using EditPictureResult? result = new WhoKnowsImageProcessor(logger).EditPicture(cover, plays, "A very long song title that goes on by An artist with a long name");

        Assert.NotNull(result);
        AssertImage(result.Stream, MagickFormat.Png, 800, 500);
    }

    [Fact]
    public async Task BiasGamePolaroid_IsAPngOfThePolaroidsSize()
    {
        using MemoryStream photo = Picture(400, 600);
        IWebService webService = Substitute.For<IWebService>();
        webService.GetStream("https://example.com/jisoo.png").Returns(photo);

        using Stream polaroid = await new BiasGameImageProcessor(webService, logger).CreatePolaroid(Idol(1, "https://example.com/jisoo.png"));

        AssertImage(polaroid, MagickFormat.Png, 280, 338);
    }

    [Fact]
    public void BiasGameMatch_PutsTwoPolaroidsSideBySide()
    {
        using MemoryStream left = Picture(280, 338);
        using MemoryStream right = Picture(280, 338);

        using Stream match = BiasGameImageProcessor.CombineImages(left, right);

        AssertImage(match, MagickFormat.Png, 610, 338);
    }

    [Fact]
    public void WinnerBracket_EachPairAndTheWinner_KeepTheBracketsSize()
    {
        using BiasGameData game = new(DiscordIds.User, TestTimes.Now.UtcDateTime);
        (uint Width, uint Height) bracketSize = Size(game.WinnerBracket);
        for (int id = 1; id <= 16; id++)
        {
            game.AddImage(id, Picture(280, 338), $"{id}.png");
        }
        game.CreatePairs();
        BiasGameWinnerBracketImageProcessor processor = new(logger);

        game.ReplaceWinnerBracket(processor.UpdateWinnerBracket(game));
        Assert.Equal(bracketSize, Size(game.WinnerBracket));

        //The final: one idol left after four rounds of halving
        for (int id = 2; id <= 16; id++)
        {
            game.RemoveItem(id);
        }
        for (int round = 0; round < 4; round++)
        {
            game.CreatePairs();
        }
        game.ReplaceWinnerBracket(processor.AddFinal(game, Idol(1, null)));

        Assert.Equal(bracketSize, Size(game.WinnerBracket));
        logger.DidNotReceiveWithAnyArgs().Error(default!, default(Exception)!);
    }

    private static MemoryStream Picture(uint width, uint height)
    {
        using MagickImage image = new(MagickColors.MediumPurple, width, height);
        MemoryStream stream = new();
        image.Write(stream, MagickFormat.Png);
        stream.Position = 0;
        return stream;
    }

    private static IdolGameResource Idol(int id, string? imageUrl)
    {
        return new IdolGameResource { IdolId = id, StageName = "Jisoo", GroupFullName = "BLACKPINK", LatestImageUrl = imageUrl };
    }

    private static void AssertImage(Stream stream, MagickFormat format, uint width, uint height)
    {
        stream.Position = 0;
        using MagickImage image = new(stream);
        Assert.Equal(format, image.Format);
        Assert.Equal((width, height), (image.Width, image.Height));
    }

    private static (uint Width, uint Height) Size(MemoryStream stream)
    {
        using MagickImage image = new(stream.ToArray());
        return (image.Width, image.Height);
    }
}
