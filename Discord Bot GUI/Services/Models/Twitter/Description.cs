using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Description
    {
        [JsonProperty("urls")]
        public List<object> Urls { get; set; }
    }
}
