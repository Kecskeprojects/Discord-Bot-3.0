using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class ExtendedEntities
{
    [JsonPropertyName("media")]
    public List<Medium> Media { get; set; }
}
