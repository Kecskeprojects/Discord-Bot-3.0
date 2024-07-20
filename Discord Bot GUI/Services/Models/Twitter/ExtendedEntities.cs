using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;
public class ExtendedEntities
{
    [JsonProperty("media")]
    public List<Medium> Media { get; set; }
}
