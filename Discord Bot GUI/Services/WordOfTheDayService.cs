using Discord_Bot.Core;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Wotd;
using RestSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Discord_Bot.Services;
public class WordOfTheDayService(BotLogger logger) : IWordOfTheDayService
{
    private readonly BotLogger logger = logger;

    private static readonly RestClient _client = new(Constant.WordOfTheDayBaseUri);

    public async Task<WotdBase> GetDataAsync(string language)
    {
        WotdBase result = null;
        try
        {
            if (Constant.WotdLanguages.ContainsKey(language.ToLower()))
            {
                RestRequest request = new(Constant.WotdLanguages[language.ToLower()]);
                RestResponse response = await _client.GetAsync(request);

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
