using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitch;
public class User
{
    [JsonProperty("data")]
    public List<UserData> Response { get; set; }
}
