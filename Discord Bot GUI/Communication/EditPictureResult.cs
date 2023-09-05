using System.IO;

namespace Discord_Bot.Communication
{
    public class EditPictureResult
    {
        public EditPictureResult(string fileName, Stream stream)
        {
            FileName = fileName;
            Stream = stream;
        }

        public string FileName { get; set; }
        public Stream Stream { get; set; }
    }
}
