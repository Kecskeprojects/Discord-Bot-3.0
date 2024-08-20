using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Twitch;

public class User
{
    [JsonPropertyName("data")]
    public List<UserData> Response { get; set; }
}
