using Discord_Bot.Communication;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Interfaces.Services
{
    public interface IPictureHandler
    {
        MemoryStream CreateBonkImage(Stream stream, int delay);
        EditPictureResult EditPicture(Stream originalImg, Dictionary<string, int> plays, string HeadText);
    }
}
