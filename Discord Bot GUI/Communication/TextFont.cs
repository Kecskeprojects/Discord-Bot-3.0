using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Communication;

//A font for drawing text with Magick.NET, used by MagickImageExtension.DrawText and MagickTools.MeasureText
public sealed record TextFont(string Family, double Size, bool Bold = false, bool Italic = false)
{
    private static readonly Lazy<HashSet<string>> installedFamilies = new(() => [.. MagickNET.FontFamilies]);

    //ImageMagick only finds fonts installed for all users, a font installed for the current user has to be used by its file path
    private readonly string? userFontFile = installedFamilies.Value.Contains(Family) ? null : FindUserFontFile(Family);

    private FontWeight Weight => Bold ? FontWeight.Bold : FontWeight.Normal;
    private FontStyleType Style => Italic ? FontStyleType.Italic : FontStyleType.Normal;

    public IDrawables<ushort> CreateDrawables()
    {
        IDrawables<ushort> drawables = userFontFile != null
            ? new Drawables().Font(userFontFile)
            : new Drawables().Font(Family, Style, Weight, FontStretch.Normal);

        return drawables.FontPointSize(Size);
    }

    public void ApplyTo(IMagickSettings<ushort> settings)
    {
        if (userFontFile != null)
        {
            settings.Font = userFontFile;
        }
        else
        {
            settings.FontFamily = Family;
            settings.FontStyle = Style;
            settings.FontWeight = Weight;
        }
        settings.FontPointsize = Size;
    }

    private static string? FindUserFontFile(string family)
    {
        //Registry values are named like "Bot Font (TrueType)" and hold the font file's full path
        using RegistryKey? fonts = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
        if (fonts == null)
        {
            return null;
        }

        string? valueName = fonts.GetValueNames().FirstOrDefault(name => name.StartsWith($"{family} (", StringComparison.OrdinalIgnoreCase));

        return valueName != null ? fonts.GetValue(valueName) as string : null;
    }
}
