using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Orig
{
    [JsonPropertyName("faces")]
    public List<Face> Faces { get; set; }
}
