using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class OriginalInfo
{
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("focus_rects")]
    public List<object> FocusRects { get; set; }
}
