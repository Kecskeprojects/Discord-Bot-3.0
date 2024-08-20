using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Face
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("h")]
    public int H { get; set; }

    [JsonPropertyName("w")]
    public int W { get; set; }
}
