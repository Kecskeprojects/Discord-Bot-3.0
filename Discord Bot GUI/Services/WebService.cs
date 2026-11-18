using Discord_Bot.Interfaces.Services;
using Discord_Bot.Tools;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace Discord_Bot.Services;

//No finalizer: it holds only a managed HttpClient, which cleans up after itself
public sealed class WebService : IWebService, IDisposable
{
    //One client for the bot's lifetime, as .NET recommends; connections are renewed every 15 minutes, so DNS changes still apply
    private readonly HttpClient client = new(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(15) })
    {
        Timeout = TimeSpan.FromMinutes(3)
    };

    public async Task<MemoryStream> GetStream(string url)
    {
        MemoryStream imageData = new();

        using (HttpResponseMessage response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead))
        {
            string? contentType = response.Content.Headers.ContentType?.MediaType;
            MagickTools.ConvertIfUnsupported(response.Content, contentType, imageData);
        }

        imageData.Position = 0;
        return imageData;
    }

    public async Task<string> GetBody(string url)
    {
        using (HttpResponseMessage response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead))
        {
            return await response.Content.ReadAsStringAsync();
        }
    }

    public bool TestConnection()
    {
        try
        {
            using (Ping ping = new())
            {
                if (ping.Send("google.com", 1000, new byte[32], new PingOptions()).Status == IPStatus.Success)
                {
                    return true;
                }
            }
        }
        catch (Exception) { }
        return false;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
