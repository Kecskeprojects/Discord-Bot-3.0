using Discord_Bot.Core;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.Wotd;
using RestSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Discord_Bot.Services
{
    public class WordOfTheDayService : IWordOfTheDayService
    {
        private readonly Logging logger;

        public WordOfTheDayService(Logging logger)
        {
            this.logger = logger;
        }

        private static string BaseUrl { get; } = "http://wotd.transparent.com/rss/";

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
                    result = (WotdBase)serializer.Deserialize(reader);
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("WordOfTheDay.cs GetDataAsync", ex.ToString());
            }

            return result;
        }
    }
}
