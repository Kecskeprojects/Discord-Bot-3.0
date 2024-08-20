using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Professional
{
    [JsonPropertyName("rest_id")]
    public string RestId { get; set; }

    [JsonPropertyName("professional_type")]
    public string ProfessionalType { get; set; }

    [JsonPropertyName("category")]
    public List<object> Category { get; set; }
}
