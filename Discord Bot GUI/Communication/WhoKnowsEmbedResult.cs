using Discord;
using System.IO;

namespace Discord_Bot.Communication;
public class WhoKnowsEmbedResult
{
    public Embed[] Embed { get; set; }
    public bool HasImage { get; set; }
    public Stream ImageData { get; set; }
    public string ImageName { get; set; }
}
