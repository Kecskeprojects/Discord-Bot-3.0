using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Hashtag
{
    [JsonProperty("indices")]
    public List<int> Indices { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}
