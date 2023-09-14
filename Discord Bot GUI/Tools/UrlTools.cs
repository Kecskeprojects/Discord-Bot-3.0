using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Tools
{
    public class UrlTools
    {
        public static List<Uri> LinkSearch(string message, bool ignoreEmbedSuppress, params string[] baseURLs)
        {
            if (string.IsNullOrWhiteSpace(message)) return null;

            message = message.Replace("www.", "");

            if (baseURLs.Any(message.Contains))
            {
                List<Uri> urls = new();

                //We check for each baseURL for each that was sent, one is expected
                foreach (string baseURL in baseURLs)
                {
                    int startIndex = 0;
                    while (startIndex != -1 && message.Length - 1 > startIndex)
                    {
                        startIndex = message.IndexOf(baseURL, startIndex);

                        if (startIndex == -1) break;

                        string beginningCut = message[startIndex..];

                        string url = beginningCut.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];

                        urls.Add(GetCleanUrl(url, ignoreEmbedSuppress));

                        startIndex++;
                    }
                }

                return urls;
            }

            return null;
        }

        public static Uri GetCleanUrl(string url, bool ignoreEmbedSuppress = true)
        {
            if (!ignoreEmbedSuppress && !url.Contains('<') && !url.Contains('>'))
            {
                url = url.Replace("<", "").Replace(">", "");
            }

            return new Uri(url.Split('?')[0]);
        }
    }
}
