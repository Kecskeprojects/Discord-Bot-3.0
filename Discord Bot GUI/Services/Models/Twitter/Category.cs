using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Twitter;
public class Category
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("icon_name")]
    public string IconName { get; set; }
}
