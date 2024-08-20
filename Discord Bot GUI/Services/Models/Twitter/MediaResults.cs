using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class MediaResults
{
    [JsonPropertyName("result")]
    public Result Result { get; set; }
}
