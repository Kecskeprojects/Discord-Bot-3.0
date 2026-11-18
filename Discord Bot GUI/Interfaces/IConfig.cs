namespace Discord_Bot.Interfaces;

/// <summary>
/// The bot's settings from Assets/config.json. Edits to the file apply without a restart,
/// and a missing value reads as "", [] or an empty dictionary, never null.
/// </summary>
public interface IConfig
{
    string Token { get; }
    string Environment { get; }
    string Img { get; }
    int Bitrate { get; }
    int VoiceWaitSeconds { get; }
    string Twitch_Client_Id { get; }
    string Spotify_Client_Id { get; }
    string Spotify_Client_Secret { get; }
    string Lastfm_API_Key { get; }
    string Lastfm_API_Secret { get; }
    Dictionary<string, string> Lastfm_Artist_Input_Replacement { get; }
    string[] Youtube_API_Keys { get; }
    string[] Youtube_Filter_Words { get; }
    bool Enable_Instagram_Embed { get; }
    bool Enable_Twitter_Embed { get; }
    string SqlConnectionString { get; }
    bool HeadlessBrowser { get; }
    bool ShowDiagnostics { get; }
}
