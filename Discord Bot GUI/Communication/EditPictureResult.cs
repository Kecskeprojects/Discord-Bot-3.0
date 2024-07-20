using System.IO;

namespace Discord_Bot.Communication;
public class EditPictureResult(string fileName, Stream stream)
{
    public string FileName { get; set; } = fileName;
    public Stream Stream { get; set; } = stream;
}
