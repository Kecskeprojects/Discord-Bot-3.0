using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Description
{
    [JsonPropertyName("urls")]
    public List<object> Urls { get; set; }
}
