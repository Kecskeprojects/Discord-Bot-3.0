using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot.Tools
{
    public static class HttpClientTools
    {
        public static async Task<Stream> GetStream(string url)
        {
            Stream imageData = null;

            using (HttpClient wc = new() { Timeout = new TimeSpan(0, 3, 0) })
            {
                imageData = await wc.GetStreamAsync(url);
            }

            return imageData;
        }
    }
}
