using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;
public class Face
{
    [JsonProperty("x")]
    public int X { get; set; }

    [JsonProperty("y")]
    public int Y { get; set; }

    [JsonProperty("h")]
    public int H { get; set; }

    [JsonProperty("w")]
    public int W { get; set; }
}
