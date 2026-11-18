using Discord_Bot.Communication;
using System.IO;

namespace Discord_Bot.Interfaces.Processors;

public interface IWhoKnowsImageProcessor
{
    //Null when the image could not be made, the error is logged
    EditPictureResult? EditPicture(Stream originalImg, Dictionary<string, int> plays, string HeadText);
}
