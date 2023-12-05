using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.MusicBrainz.ArtistLookup;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class MusicBrainzAPI(Logging logger) : IMusicBrainzAPI
    {
        private static string BaseUrl { get; } = "https://musicbrainz.org/ws/2/";
        private static readonly RestClient _client = new(BaseUrl);
        private readonly Logging logger = logger;

        //https://musicbrainz.org/ws/2/artist/[artistMBID]?inc=url-rels&fmt=json
        public async Task<string> GetArtistSpotifyUrlAsync(string mbid)
        {
            try
            {
                string query = $"artist/{mbid}?inc=url-rels&fmt=json";
                logger.Query("Musicbrainz query Url:\n" + BaseUrl + query);
                RestRequest request = new(query);
                RestResponse resultJSON = await _client.GetAsync(request);
                ArtistLookup deserialized = JsonConvert.DeserializeObject<ArtistLookup>(resultJSON.Content);

                Url spotifyUrl = deserialized.Relations.FirstOrDefault(x => x.Url != null && x.Url.Resource.Contains("spotify"))?.Url;
                return spotifyUrl?.Resource;
            }
            catch (Exception ex)
            {
                logger.Error("MusicBrainzAPI.cs GetArtistSpotifyUrl", ex.ToString());
            }

            return null;
        }
    }
}
