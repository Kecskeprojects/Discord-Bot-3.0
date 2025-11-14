using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LastFmApi.Communication;

public class BaseResponse
{
    [JsonProperty("message")]
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonProperty("error")]
    [JsonPropertyName("error")]
    public int? Error { get; set; }
}
