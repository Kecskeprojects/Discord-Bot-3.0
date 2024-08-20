using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Hashtag
{
    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}
