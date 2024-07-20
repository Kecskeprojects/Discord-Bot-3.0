using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;
public class Url2
{
    [JsonProperty("display_url")]
    public string DisplayUrl { get; set; }

    [JsonProperty("expanded_url")]
    public string ExpandedUrl { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("indices")]
    public List<int> Indices { get; set; }
}
