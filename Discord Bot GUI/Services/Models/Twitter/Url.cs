using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Url
{
    [JsonProperty("urls")]
    public List<Url> Urls { get; set; }
}
