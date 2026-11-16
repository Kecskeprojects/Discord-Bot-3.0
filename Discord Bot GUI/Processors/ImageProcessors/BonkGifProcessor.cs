using Discord_Bot.Interfaces.Core;
using Discord_Bot.Properties;
using Discord_Bot.Tools;
using Discord_Bot.Tools.Extensions;
using ImageMagick;
using System;
using System.Drawing;
using System.IO;

namespace Discord_Bot.Processors.ImageProcessors;

public class BonkGifProcessor(IBotLogger logger)
{
    private readonly IBotLogger logger = logger;

    //Null when the gif could not be made, the error is logged
    public MemoryStream? CreateBonkImage(Stream stream, int delay)
    {
        try
        {
            // Image dimensions of the gif.
            const uint width = 1000;
            const uint height = 1000;

            using MagickImage profile = new(stream);
            profile.ResizeExact(400, 400);
            profile.ApplyRoundedCorners(200);

            //The collection disposes the frames added to it
            using MagickImageCollection gif = new();
            foreach (Bitmap resource in new[] { Resource.winter0, Resource.winter1 })
            {
                using MagickImage winter = MagickTools.LoadResource(resource);
                winter.ResizeExact(width, height);

                //The profile picture goes under the winter frame, which is transparent where the picture shows through
                //The ImageSharp version filled with Rgba32(1, 1, 1, 0.5f), which picks the float constructor and means half transparent white
                //GIF has no partial transparency, so users always saw opaque white
                MagickImage frame = new(MagickColors.White, width, height);
                frame.Composite(profile, 10, 500, CompositeOperator.Over);
                frame.Composite(winter, 0, 0, CompositeOperator.Over);

                //Delay is in hundredths of a second, 0 iterations loops forever
                frame.AnimationDelay = (uint) delay;
                frame.AnimationIterations = 0;
                frame.GifDisposeMethod = GifDisposeMethod.Background;

                gif.Add(frame);
            }

            MemoryStream gifStream = new();
            gif.Write(gifStream, MagickFormat.Gif);

            return gifStream;
        }
        catch (Exception ex)
        {
            logger.Error("BonkGifProcessor.cs CreateBonkImage", ex);
        }

        return null;
    }
}
