using Discord_Bot.Interfaces;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Mocks.Config;

//A config.json in memory: test keys by default, and every value settable by a test
public sealed class TestConfig : IConfig
{
    public string Token { get; set; } = "";
    public string Environment { get; set; } = "testing";
    public string Img { get; set; } = "";
    public int Bitrate { get; set; }
    public int VoiceWaitSeconds { get; set; } = 60;
    public string Twitch_Client_Id { get; set; } = "";
    public string Spotify_Client_Id { get; set; } = "spotify-test-id";
    public string Spotify_Client_Secret { get; set; } = "spotify-test-secret";
    public string Lastfm_API_Key { get; set; } = TestValues.LastFmApiKey;
    public string Lastfm_API_Secret { get; set; } = "";
    public Dictionary<string, string> Lastfm_Artist_Input_Replacement { get; set; } = [];
    public string[] Youtube_API_Keys { get; set; } = [TestValues.YoutubeApiKey];
    public string[] Youtube_Filter_Words { get; set; } = [];
    public bool Enable_Instagram_Embed { get; set; }
    public bool Enable_Twitter_Embed { get; set; }
    //Nothing listens there, so a test that reaches the database by mistake fails within a second
    public string SqlConnectionString { get; set; } = "Server=127.0.0.1,1;Database=DiscordBotTests;Connect Timeout=1";
    public bool HeadlessBrowser { get; set; } = true;
    public bool ShowDiagnostics { get; set; }
}
