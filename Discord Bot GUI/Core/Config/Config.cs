using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Discord_Bot.Core.Config
{
    public class Config
    {
        private readonly IConfiguration Configuration;
        public struct ConfigModel
        {
            public string token;
            public string img;
            public int bitrate;
            public string twitch_Client_Id;
            public string spotify_Client_Id;
            public string spotify_Client_Secret;
            public string lastfm_API_Key;
            public string lastfm_API_Secret;
            public string[] youtube_API_Keys;
            public string[] youtube_Filter_Words;
            public bool enable_Instagram_Embed;
            public string[] instagram_Username;
            public string[] instagram_Password;
            public bool enable_Twitter_Embed;
            public string sql_connection_string;
        }

        public Config()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
            if (!File.Exists(Path.Combine(path, "config.json")))
            {
                ConfigModel conf = new()
                {
                    token = "",
                    img = "",
                    bitrate = 0,
                    twitch_Client_Id = "",
                    spotify_Client_Id = "",
                    spotify_Client_Secret = "",
                    lastfm_API_Key = "",
                    lastfm_API_Secret = "",
                    youtube_API_Keys = Array.Empty<string>(),
                    youtube_Filter_Words = Array.Empty<string>(),
                    enable_Instagram_Embed = false,
                    instagram_Username = Array.Empty<string>(),
                    instagram_Password = Array.Empty<string>(),
                    enable_Twitter_Embed = false
                };

                using StreamWriter sw = File.AppendText(Path.Combine(path, "config.json"));
                sw.WriteLine(JsonConvert.SerializeObject(conf));

                return;
            }

            Configuration = new ConfigurationBuilder()
             .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Assets"))
             .AddJsonFile("config.json", optional: false, reloadOnChange: true)
             .Build();
        }

        #region Config values
        public string Token => Configuration.GetSection("token").Get<string>();

        public string Img => Configuration.GetSection("img").Get<string>();

        public int Bitrate => Configuration.GetSection("bitrate").Get<int>();

        public string Twitch_Client_Id => Configuration.GetSection("twitch_client_id").Get<string>();

        public string Spotify_Client_Id => Configuration.GetSection("spotify_client_id").Get<string>();

        public string Spotify_Client_Secret => Configuration.GetSection("spotify_client_secret").Get<string>();

        public string Lastfm_API_Key => Configuration.GetSection("lastfm_api_key").Get<string>();

        public string Lastfm_API_Secret => Configuration.GetSection("lastfm_api_secret").Get<string>();

        public string[] Youtube_API_Keys => Configuration.GetSection("youtube_api_keys").Get<string[]>();

        public string[] Youtube_Filter_Words => Configuration.GetSection("youtube_filter_words").Get<string[]>();

        public bool Enable_Instagram_Embed => Configuration.GetSection("enable_instagram_embed").Get<bool>();

        public string[] Instagram_Username => Configuration.GetSection("instagram_username").Get<string[]>();

        public string[] Instagram_Password => Configuration.GetSection("instagram_password").Get<string[]>();

        public bool Enable_Twitter_Embed => Configuration.GetSection("enable_twitter_embed").Get<bool>();

        public string SqlConnectionString => Configuration.GetSection("sql_connection_string").Get<string>();
        #endregion

    }
}
