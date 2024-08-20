using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class ExtendedEntities
{
    [JsonPropertyName("media")]
    public List<Medium> Media { get; set; }
}
