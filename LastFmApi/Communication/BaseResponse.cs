using Newtonsoft.Json;

namespace LastFmApi.Communication;
public class BaseResponse
{
    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("error")]
    public int? Error { get; set; }
}
