using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Url
{
    [JsonPropertyName("urls")]
    public List<Url> Urls { get; set; }
}
