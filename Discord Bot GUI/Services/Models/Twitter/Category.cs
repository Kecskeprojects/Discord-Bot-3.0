using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Category
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("icon_name")]
    public string IconName { get; set; }
}
