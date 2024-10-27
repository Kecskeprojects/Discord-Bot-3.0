using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Discord_Bot.Core.Configuration;

public class Config
{
    private readonly IConfiguration Configuration;
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
        internal Dictionary<string, string> lastfm_artist_input_replacement;
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
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
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

            using StreamWriter sw = File.AppendText(Path.Combine(assetDir, "config.json"));
            sw.WriteLine(JsonConvert.SerializeObject(conf));

            return;
        }

        Configuration = new ConfigurationBuilder()
         .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Assets"))
         .AddJsonFile("config.json", optional: false, reloadOnChange: true)
         .Build();
    }

    #region Config Values
    public string Token => Configuration.GetSection("token").Get<string>();

    public string Environment => Configuration.GetSection("environment").Get<string>();

    public string Img => Configuration.GetSection("img").Get<string>();

    public int Bitrate => Configuration.GetSection("bitrate").Get<int>();

    public int VoiceWaitSeconds => Configuration.GetSection("voice_wait_seconds").Get<int>();

    public string Twitch_Client_Id => Configuration.GetSection("twitch_client_id").Get<string>();

    public string Spotify_Client_Id => Configuration.GetSection("spotify_client_id").Get<string>();

    public string Spotify_Client_Secret => Configuration.GetSection("spotify_client_secret").Get<string>();

    public string Lastfm_API_Key => Configuration.GetSection("lastfm_api_key").Get<string>();

    public string Lastfm_API_Secret => Configuration.GetSection("lastfm_api_secret").Get<string>();

    public Dictionary<string, string> Lastfm_Artist_Input_Replacement => Configuration.GetSection("lastfm_artist_input_replacement").Get<Dictionary<string, string>>();

    public string[] Youtube_API_Keys => Configuration.GetSection("youtube_api_keys").Get<string[]>();

    public string[] Youtube_Filter_Words => Configuration.GetSection("youtube_filter_words").Get<string[]>();

    public bool Enable_Instagram_Embed => Configuration.GetSection("enable_instagram_embed").Get<bool>();

    public bool Enable_Twitter_Embed => Configuration.GetSection("enable_twitter_embed").Get<bool>();

    public string SqlConnectionString => Configuration.GetSection("sql_connection_string").Get<string>();

    public bool HeadlessBrowser => Configuration.GetSection("headless_browser").Get<bool>();

    public bool ShowDiagnostics => Configuration.GetSection("show_diagnostics").Get<bool>();
    #endregion

}
