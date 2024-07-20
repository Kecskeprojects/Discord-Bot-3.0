using System;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Discord_Bot.Tools;
public static class WebTools
{
    public static async Task<MemoryStream> GetStream(string url)
    {
        MemoryStream imageData = new();

        using (HttpClient wc = new()
        {
            Timeout = new TimeSpan(0, 3, 0)
        })
        {
            HttpResponseMessage response = await wc.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            string contentType = response.Content.Headers.ContentType.MediaType;
            MagickTools.ConvertIfUnsupported(response.Content, contentType, imageData);
        }

        imageData.Position = 0;
        return imageData;
    }

    public static bool TestConnection()
    {
        try
        {
            if (new Ping().Send("google.com", 1000, new byte[32], new PingOptions()).Status == IPStatus.Success)
            {
                return true;
            }
        }
        catch (Exception) { }
        return false;
    }
}
