using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Small
{
    [JsonPropertyName("faces")]
    public List<Face> Faces { get; set; }

    [JsonPropertyName("h")]
    public int H { get; set; }

    [JsonPropertyName("w")]
    public int W { get; set; }

    [JsonPropertyName("resize")]
    public string Resize { get; set; }
}
