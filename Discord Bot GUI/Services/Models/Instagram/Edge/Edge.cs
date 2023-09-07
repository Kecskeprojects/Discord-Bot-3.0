using Newtonsoft.Json;

namespace Discord_Bot.Services.Models.Instagram.Edge
{
    public class Edge
    {
        [JsonProperty("node")]
        public Node Node { get; set; }
    }
}
