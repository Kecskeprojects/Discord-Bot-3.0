using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Hashtag
{
    [JsonProperty("indices")]
    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; }

    [JsonProperty("text")]
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
