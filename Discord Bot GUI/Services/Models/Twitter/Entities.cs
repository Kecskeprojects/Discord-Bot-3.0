using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Entities
{
    [JsonPropertyName("description")]
    public Description Description { get; set; }

    [JsonPropertyName("hashtags")]
    public List<object> Hashtags { get; set; }

    [JsonPropertyName("media")]
    public List<Medium> Media { get; set; }

    [JsonPropertyName("symbols")]
    public List<object> Symbols { get; set; }

    [JsonPropertyName("timestamps")]
    public List<object> Timestamps { get; set; }

    [JsonPropertyName("urls")]
    public List<object> Urls { get; set; }

    [JsonPropertyName("user_mentions")]
    public List<object> UserMentions { get; set; }
}
