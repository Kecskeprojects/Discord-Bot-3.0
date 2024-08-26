using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitter;

public class Professional
{
    [JsonProperty("rest_id")]
    [JsonPropertyName("rest_id")]
    public string RestId { get; set; }

    [JsonProperty("professional_type")]
    [JsonPropertyName("professional_type")]
    public string ProfessionalType { get; set; }

    [JsonProperty("category")]
    [JsonPropertyName("category")]
    public List<object> Category { get; set; }
}
