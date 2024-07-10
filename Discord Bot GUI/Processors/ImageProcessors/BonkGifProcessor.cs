using Discord_Bot.Core;
using Discord_Bot.Properties;
using Discord_Bot.Tools.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;

namespace Discord_Bot.Processors.ImageProcessors;

public class BonkGifProcessor(BotLogger logger)
{
    private readonly BotLogger logger = logger;

    public MemoryStream CreateBonkImage(Stream stream, int delay)
    {
        try
        {
            // Image dimensions of the gif.
            const int width = 1000;
            const int height = 1000;

            using (Image profile = Image.Load<Rgba32>(stream))
            {
                profile.Mutate(x => x.Resize(400, 400).ApplyRoundedCorners(200));

                using MemoryStream winter0Stream = new();
                Resource.winter0.Save(winter0Stream, System.Drawing.Imaging.ImageFormat.Png);
                using Image winter0 = Image.Load<Rgba32>(winter0Stream.ToArray());
                winter0.Mutate(x => x.Resize(width, height));

                using MemoryStream winter1Stream = new();
                Resource.winter1.Save(winter1Stream, System.Drawing.Imaging.ImageFormat.Png);
                using Image winter1 = Image.Load<Rgba32>(winter1Stream.ToArray());
                winter1.Mutate(x => x.Resize(width, height));

                // For demonstration: use images with different colors.
                Image[] images = [null, null];
                Image[] winterFrame = [winter0, winter1];
                for (int i = 0; i < 2; i++)
                {
                    Image background = new Image<Rgba32>(width, height, new Rgba32(1, 1, 1, 0.5f));
                    background.Mutate(x => x.DrawImage(profile, backgroundLocation: new Point(10, 500), 1));
                    background.Mutate(x => x.DrawImage(winterFrame[i], backgroundLocation: new Point(0, 0), 1));

                    images[i] = background;
                }

                // Create empty image.
                using Image<Rgba32> gif = new(width, height, new Rgba32(0, 0, 0, 0));

                GifMetadata gifMetaData = gif.Metadata.GetGifMetadata();
                gifMetaData.RepeatCount = 0;

                // Set the delay until the next image is displayed.
                GifFrameMetadata metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();
                for (int i = 0; i < images.Length; i++)
                {
                    // Set the delay until the next image is displayed.
                    metadata = images[i].Frames.RootFrame.Metadata.GetGifMetadata();
                    metadata.FrameDelay = delay;
                    metadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;

                    // Add the color image to the gif.
                    gif.Frames.AddFrame(images[i].Frames.RootFrame);
                }
                gif.Frames.RemoveFrame(0);
                images.ToList().ForEach(x => x.Dispose());

                // Save the final result.
                MemoryStream gifStream = new();
                gif.SaveAsGif(gifStream);

                return gifStream;
            }
        }
        catch (Exception ex)
        {
            logger.Error("BonkGifProcessor.cs CreateBonkImage", ex);
        }

        return null;
    }
}
