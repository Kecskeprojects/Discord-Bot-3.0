using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Large
{
    [JsonProperty("faces")]
    [JsonPropertyName("faces")]
    public List<Face> Faces { get; set; }

    [JsonProperty("h")]
    [JsonPropertyName("h")]
    public int H { get; set; }

    [JsonProperty("w")]
    [JsonPropertyName("w")]
    public int W { get; set; }

    [JsonProperty("resize")]
    [JsonPropertyName("resize")]
    public string Resize { get; set; }
}
