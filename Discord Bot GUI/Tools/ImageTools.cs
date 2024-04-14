using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace Discord_Bot.Tools
{
    public class ImageTools
    {

        public static void CorrectImageRatio(Image mainImage, Image AlbumImage)
        {
            //If the Cover image is too wide or high, we crop it depending on which of the two it is
            if (mainImage.Height / mainImage.Width > 1.2 || mainImage.Height / mainImage.Width < 0.8)
            {
                int difference;
                if (mainImage.Width > mainImage.Height)
                {
                    difference = mainImage.Width - mainImage.Height;

                    mainImage.Mutate(x => x.Crop(new Rectangle(difference / 2, 0, mainImage.Width - difference, mainImage.Height)));
                    AlbumImage.Mutate(x => x.Crop(new Rectangle(difference / 2, 0, AlbumImage.Width - difference, AlbumImage.Height)));
                }
                else
                {
                    difference = mainImage.Height - mainImage.Width;

                    mainImage.Mutate(x => x.Crop(new Rectangle(0, difference / 2, mainImage.Width, mainImage.Height - difference)));
                    AlbumImage.Mutate(x => x.Crop(new Rectangle(0, difference / 2, AlbumImage.Width, AlbumImage.Height - difference)));
                }

            }
        }
        public static Tuple<Color, Color> GetContrastAndDominantColors(Image<Rgba32> image)
        {
            //Use nearest neighbor sampling to simplify image and resize it to a 100 pixel wide image with near equal aspect ratio
            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Sampler = KnownResamplers.NearestNeighbor,
                    Size = new Size(100, 0)
                }
                )
            );

            //Declare variables
            int r = 0, g = 0, b = 0;
            int totalPixels = 0;

            //Go through the image pixel line by pixel line
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Rgba32 pixel = image[x, y];

                    r += Convert.ToInt32(pixel.R);
                    g += Convert.ToInt32(pixel.G);
                    b += Convert.ToInt32(pixel.B);

                    totalPixels++;
                }
            }

            //Putpixel numbers into the 255 range by dividing it with the total pixels checked
            r /= totalPixels;
            g /= totalPixels;
            b /= totalPixels;

            //We get the dominant color
            Rgba32 DominantColor = new((byte)r, (byte)g, (byte)b, 255);

            //By subtracting the values from 255 we get the exact opposite of the color, making it the most visible color
            r = 255 - r; g = 255 - g; b = 255 - b;
            Rgba32 ContrastColor = new((byte)r, (byte)g, (byte)b, 255);

            //Return the two colors in a tuple
            return new Tuple<Color, Color>(DominantColor, ContrastColor);
        }

        public static Rgba32 BlackOrWhite(Rgba32 color)
        {
            //Formula deciding whether black or white would look more visible
            double l = (0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B);

            return l < 127.5 ? Color.White : Color.Black;
        }
    }
}
