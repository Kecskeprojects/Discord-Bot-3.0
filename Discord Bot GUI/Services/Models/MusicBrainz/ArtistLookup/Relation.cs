using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.MusicBrainz.ArtistLookup
{
    public class Relation
    {
        [JsonPropertyName("attribute-ids")]
        public object AttributeIds { get; set; }

        [JsonPropertyName("end")]
        public string End { get; set; }

        [JsonPropertyName("url")]
        public Url Url { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("target-type")]
        public string TargetType { get; set; }

        [JsonPropertyName("begin")]
        public object Begin { get; set; }

        [JsonPropertyName("target-credit")]
        public string TargetCredit { get; set; }

        [JsonPropertyName("source-credit")]
        public string SourceCredit { get; set; }

        [JsonPropertyName("ended")]
        public bool Ended { get; set; }

        [JsonPropertyName("attributes")]
        public List<object> Attributes { get; set; }

        [JsonPropertyName("attribute-values")]
        public object AttributeValues { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("type-id")]
        public string TypeId { get; set; }
    }
}
