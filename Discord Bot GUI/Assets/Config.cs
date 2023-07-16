using System.IO;
using Microsoft.Extensions.Configuration;

namespace Discord_Bot.Assets
{
    public static class Config
    {
        private static readonly IConfiguration Configuration;

        static Config()
        {
            Configuration = new ConfigurationBuilder()
             .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Assets"))
             .AddJsonFile("config.json", optional: false, reloadOnChange: true)
             .Build();
        }

        #region Config values
        public static string Token { get { return Configuration.GetSection("token").Get<string>(); } }

        public static string Img { get { return Configuration.GetSection("img").Get<string>(); } }

        public static int Bitrate { get { return Configuration.GetSection("bitrate").Get<int>(); } }

        public static string Twitch_Client_Id { get { return Configuration.GetSection("twitch_client_id").Get<string>(); } }

        public static string Spotify_Client_Id { get { return Configuration.GetSection("spotify_client_id").Get<string>(); } }

        public static string Spotify_Client_Secret { get { return Configuration.GetSection("spotify_client_secret").Get<string>(); } }

        public static string Lastfm_API_Key { get { return Configuration.GetSection("lastfm_api_key").Get<string>(); } }

        public static string Lastfm_API_Secret { get { return Configuration.GetSection("lastfm_api_secret").Get<string>(); } }

        public static string[] Youtube_API_Keys { get { return Configuration.GetSection("youtube_api_keys").Get<string[]>(); } }

        public static string[] Youtube_Filter_Words { get { return Configuration.GetSection("youtube_filter_words").Get<string[]>(); } }

        public static bool Enable_Instagram_Embed { get { return Configuration.GetSection("enable_instagram_embed").Get<bool>(); } }

        public static string[] Instagram_Username { get { return Configuration.GetSection("instagram_username").Get<string[]>(); } }

        public static string[] Instagram_Password { get { return Configuration.GetSection("instagram_password").Get<string[]>(); } }

        public static bool Enable_Twitter_Embed { get { return Configuration.GetSection("enable_twitter_embed").Get<bool>(); } }

        public static  string SqlConnectionString { get { return Configuration.GetSection("sql_connection_string").Get<string>(); } }
        #endregion

    }
}
