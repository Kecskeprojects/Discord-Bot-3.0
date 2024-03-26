using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Tools
{
    public class UrlTools
    {
        private static readonly char[] whiteSpaceSeparator = [' ', '\n'];

        public static List<Uri> LinkSearch(string message, bool ignoreEmbedSuppress, params string[] baseURLs)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            message = message.Replace("www.", "");

            if (baseURLs.Any(message.Contains))
            {
                List<Uri> urls = [];

                //We check for each baseURL for each that was sent, one is expected
                foreach (string baseURL in baseURLs)
                {
                    int startIndex = 0;
                    while (startIndex != -1 && message.Length - 1 > startIndex)
                    {
                        startIndex = message.IndexOf(baseURL, startIndex);

                        if (startIndex == -1)
                        {
                            break;
                        }

                        string beginningCut = message[startIndex..];

                        string url = beginningCut.Split(whiteSpaceSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];

                        if (url.Contains('<') || url.Contains('>'))
                        {
                            if (!ignoreEmbedSuppress)
                            {
                                startIndex++;
                                continue;
                            }
                            url = url.Replace("<", "").Replace(">", "");
                        }

                        urls.Add(new Uri(url.Split('?')[0]));

                        startIndex++;
                    }
                }

                return urls;
            }

            return null;
        }

        public static string SanitizeText(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            int startIndex = 0;
            while (startIndex != -1 && message.Length - 1 > startIndex)
            {
                startIndex = message.IndexOf("https://", startIndex);

                if (startIndex == -1)
                {
                    break;
                }

                string beginningCut = message[startIndex..];

                string url = beginningCut.Split(whiteSpaceSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];

                message = message.Replace(url, $"<{url}>");

                startIndex += url.Length + 2;
            }

            return message;
        }
    }
}
