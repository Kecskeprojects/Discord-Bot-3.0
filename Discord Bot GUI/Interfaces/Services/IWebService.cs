using System.IO;

namespace Discord_Bot.Interfaces.Services;

/// <summary>
/// Plain web requests: files and pages by URL, and whether the internet can be reached.
/// </summary>
public interface IWebService
{
    //Downloads a file into memory, converting AVIF images to PNG; the caller disposes the stream
    Task<MemoryStream> GetStream(string url);
    Task<string> GetBody(string url);
    //A ping to google.com
    bool TestConnection();
}
