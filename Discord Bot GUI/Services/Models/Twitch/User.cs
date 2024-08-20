using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitch;

public class User
{
    [JsonPropertyName("data")]
    public List<UserData> Response { get; set; }
}
