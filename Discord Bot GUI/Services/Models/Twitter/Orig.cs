using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Orig
{
    [JsonPropertyName("faces")]
    public List<Face> Faces { get; set; }
}
