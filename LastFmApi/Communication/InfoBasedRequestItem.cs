namespace LastFmApi.Communication;

public class InfoBasedRequestItem(string type, string username, string apiKey, string artist)
{
    public string Type { get; set; } = type;
    public string Username { get; set; } = username;
    public string ApiKey { get; set; } = apiKey;
    public string Artist { get; set; } = artist;
    public string Track { get; set; }
    public string Album { get; set; }
}
