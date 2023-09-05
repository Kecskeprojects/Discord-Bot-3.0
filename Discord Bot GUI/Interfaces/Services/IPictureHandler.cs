using Discord_Bot.Communication;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Interfaces.Services
{
    public interface IPictureHandler
    {
        EditPictureResult EditPicture(Stream originalImg, Dictionary<string, int> plays, string HeadText);
    }
}
