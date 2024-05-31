using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter;

public class Professional
{
    [JsonProperty("rest_id")]
    public string RestId { get; set; }

    [JsonProperty("professional_type")]
    public string ProfessionalType { get; set; }

    [JsonProperty("category")]
    public List<object> Category { get; set; }
}
