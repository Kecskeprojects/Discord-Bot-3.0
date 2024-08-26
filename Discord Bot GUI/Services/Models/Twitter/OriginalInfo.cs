using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class OriginalInfo
{
    [JsonProperty("height")]
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonProperty("width")]
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonProperty("focus_rects")]
    [JsonPropertyName("focus_rects")]
    public List<object> FocusRects { get; set; }
}
