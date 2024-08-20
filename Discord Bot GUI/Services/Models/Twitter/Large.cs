using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Large
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
