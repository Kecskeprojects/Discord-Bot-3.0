using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class PageInfo
{
    [JsonPropertyName("end_cursor")]
    public string EndCursor { get; set; }

    [JsonPropertyName("has_next_page")]
    public bool HasNextPage { get; set; }
}
