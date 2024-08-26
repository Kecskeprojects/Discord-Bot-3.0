using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Entities
{
    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public Description Description { get; set; }

    [JsonProperty("hashtags")]
    [JsonPropertyName("hashtags")]
    public List<object> Hashtags { get; set; }

    [JsonProperty("media")]
    [JsonPropertyName("media")]
    public List<Medium> Media { get; set; }

    [JsonProperty("symbols")]
    [JsonPropertyName("symbols")]
    public List<object> Symbols { get; set; }

    [JsonProperty("timestamps")]
    [JsonPropertyName("timestamps")]
    public List<object> Timestamps { get; set; }

    [JsonProperty("urls")]
    [JsonPropertyName("urls")]
    public List<object> Urls { get; set; }

    [JsonProperty("user_mentions")]
    [JsonPropertyName("user_mentions")]
    public List<object> UserMentions { get; set; }
}
