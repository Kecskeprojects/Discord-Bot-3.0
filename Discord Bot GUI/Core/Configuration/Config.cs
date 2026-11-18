using Discord_Bot.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace Discord_Bot.Core.Configuration;

public class Config : IConfig
{
    private readonly IConfiguration configuration;
    public struct ConfigModel
    {
        public string token;
        public string environment;
        public string img;
        public int bitrate;
        public int voice_wait_seconds;
        public string twitch_Client_Id;
        public string spotify_Client_Id;
        public string spotify_Client_Secret;
        public string lastfm_API_Key;
        public string lastfm_API_Secret;
        public Dictionary<string, string> lastfm_artist_input_replacement;
        public string[] youtube_API_Keys;
        public string[] youtube_Filter_Words;
        public bool enable_Instagram_Embed;
        public bool enable_Twitter_Embed;
        public string sql_connection_string;
        public bool headless_browser;
        public bool show_diagnostics;
    }

    public Config()
    {
        string assetDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets");

        if (!Directory.Exists(assetDir))
        {
            _ = Directory.CreateDirectory(assetDir);
        }

        if (!File.Exists(Path.Combine(assetDir, "config.json")))
        {
            ConfigModel conf = new()
            {
                token = "",
                environment = "testing",
                img = "",
                bitrate = 0,
                voice_wait_seconds = 0,
                twitch_Client_Id = "",
                spotify_Client_Id = "",
                spotify_Client_Secret = "",
                lastfm_API_Key = "",
                lastfm_API_Secret = "",
                lastfm_artist_input_replacement = [],
                youtube_API_Keys = [],
                youtube_Filter_Words = [],
                enable_Instagram_Embed = false,
                enable_Twitter_Embed = false,
                sql_connection_string = "",
                headless_browser = true,
                show_diagnostics = true,
            };

            //Written and closed before loading below, so an empty config is read instead of leaving configuration null
            File.WriteAllText(Path.Combine(assetDir, "config.json"), JsonConvert.SerializeObject(conf, Formatting.Indented));
        }

        configuration = new ConfigurationBuilder()
         .SetBasePath(assetDir)
         .AddJsonFile("config.json", optional: false, reloadOnChange: true)
         .Build();
    }

    //A missing value comes back empty, never null. The binder also reads an empty JSON object ({}) as null,
    //which is what the generated config.json has for lastfm_artist_input_replacement
    #region Config Values
    public string Token => configuration.GetSection("token").Get<string>() ?? "";

    public string Environment => configuration.GetSection("environment").Get<string>() ?? "";

    public string Img => configuration.GetSection("img").Get<string>() ?? "";

    public int Bitrate => configuration.GetSection("bitrate").Get<int>();

    public int VoiceWaitSeconds => configuration.GetSection("voice_wait_seconds").Get<int>();

    public string Twitch_Client_Id => configuration.GetSection("twitch_client_id").Get<string>() ?? "";

    public string Spotify_Client_Id => configuration.GetSection("spotify_client_id").Get<string>() ?? "";

    public string Spotify_Client_Secret => configuration.GetSection("spotify_client_secret").Get<string>() ?? "";

    public string Lastfm_API_Key => configuration.GetSection("lastfm_api_key").Get<string>() ?? "";

    public string Lastfm_API_Secret => configuration.GetSection("lastfm_api_secret").Get<string>() ?? "";

    public Dictionary<string, string> Lastfm_Artist_Input_Replacement => configuration.GetSection("lastfm_artist_input_replacement").Get<Dictionary<string, string>>() ?? [];

    public string[] Youtube_API_Keys => configuration.GetSection("youtube_api_keys").Get<string[]>() ?? [];

    public string[] Youtube_Filter_Words => configuration.GetSection("youtube_filter_words").Get<string[]>() ?? [];

    public bool Enable_Instagram_Embed => configuration.GetSection("enable_instagram_embed").Get<bool>();

    public bool Enable_Twitter_Embed => configuration.GetSection("enable_twitter_embed").Get<bool>();

    public string SqlConnectionString => configuration.GetSection("sql_connection_string").Get<string>() ?? "";

    public bool HeadlessBrowser => configuration.GetSection("headless_browser").Get<bool>();

    public bool ShowDiagnostics => configuration.GetSection("show_diagnostics").Get<bool>();
    #endregion

}
