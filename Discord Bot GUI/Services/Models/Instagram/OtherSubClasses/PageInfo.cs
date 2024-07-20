using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Instagram.OtherSubClasses;

public class PageInfo
{
    [JsonProperty("end_cursor")]
    public string EndCursor { get; set; }

    [JsonProperty("has_next_page")]
    public bool HasNextPage { get; set; }
}
