using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.Twitter
{
    public class Orig
    {
        [JsonProperty("faces")]
        public List<Face> Faces { get; set; }
    }
}
