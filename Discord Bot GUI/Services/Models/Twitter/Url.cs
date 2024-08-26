using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Url
{
    [JsonProperty("urls")]
    [JsonPropertyName("urls")]
    public List<Url> Urls { get; set; }
}
