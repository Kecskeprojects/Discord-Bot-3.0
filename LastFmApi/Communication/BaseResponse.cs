using System.Text.Json.Serialization;

namespace LastFmApi.Communication;
public class BaseResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("error")]
    public int? Error { get; set; }
}
