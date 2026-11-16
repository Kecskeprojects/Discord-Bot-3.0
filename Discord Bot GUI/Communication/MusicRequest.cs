namespace Discord_Bot.Communication;

public class MusicRequest(string uRL, string title, string thumbnail, string duration, string user)
{
    public string URL { get; set; } = uRL;
    public string Title { get; set; } = title;
    public string Thumbnail { get; set; } = thumbnail;
    public string Duration { get; set; } = duration;
    public string User { get; set; } = user;
}
