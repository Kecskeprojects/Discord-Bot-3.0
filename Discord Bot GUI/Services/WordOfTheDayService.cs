#nullable enable

using Discord_Bot.Core;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Wotd;
using RestSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Discord_Bot.Services;

public class WordOfTheDayService(BotLogger logger) : IWordOfTheDayService
{
    private readonly BotLogger logger = logger;

    private static readonly RestClient client = new(Constant.WordOfTheDayBaseUri);

    public async Task<WotdBase?> GetDataAsync(string language)
    {
        WotdBase? result = null;
        try
        {
            if (Constant.WotdLanguages.ContainsKey(language.ToLower()))
            {
                RestRequest request = new(Constant.WotdLanguages[language.ToLower()]);
                RestResponse response = await client.GetAsync(request);

                XmlSerializer serializer = new(typeof(WotdBase));
                using StringReader stringReader = new(response.Content ?? "");
                //The feed comes from an external site, so a DTD in it is skipped instead of processed
                using XmlReader reader = XmlReader.Create(stringReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
                result = (WotdBase?) serializer.Deserialize(reader);
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
