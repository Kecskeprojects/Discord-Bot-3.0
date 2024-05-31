using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Wotd;
using RestSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Discord_Bot.Services;

public class WordOfTheDayService(Logging logger) : IWordOfTheDayService
{
    private readonly Logging logger = logger;

    private static Uri BaseUrl { get; } = new("http://wotd.transparent.com/rss/");

    public async Task<WotdBase> GetDataAsync(string language)
    {
        WotdBase result = null;
        try
        {
            if (StaticLists.WotdLanguages.ContainsKey(language.ToLower()))
            {
                RestClient client = new(BaseUrl);
                RestRequest request = new(StaticLists.WotdLanguages[language.ToLower()]);
                RestResponse response = await client.GetAsync(request);

                XmlSerializer serializer = new(typeof(WotdBase));
                using StringReader reader = new(response.Content);
                result = (WotdBase) serializer.Deserialize(reader);
            }
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error("WordOfTheDay.cs GetDataAsync", ex);
        }

        return result;
    }
}
