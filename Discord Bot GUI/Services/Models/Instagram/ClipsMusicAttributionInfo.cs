using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Discord_Bot.Services.Models.Instagram;
public class ClipsMusicAttributionInfo
{
    [JsonProperty("artist_name")]
    [JsonPropertyName("artist_name")]
    public string ArtistName { get; set; }

    [JsonProperty("song_name")]
    [JsonPropertyName("song_name")]
    public string SongName { get; set; }

    [JsonProperty("uses_original_audio")]
    [JsonPropertyName("uses_original_audio")]
    public bool UsesOriginalAudio { get; set; }

    [JsonProperty("should_mute_audio")]
    [JsonPropertyName("should_mute_audio")]
    public bool ShouldMuteAudio { get; set; }

    [JsonProperty("should_mute_audio_reason")]
    [JsonPropertyName("should_mute_audio_reason")]
    public string ShouldMuteAudioReason { get; set; }

    [JsonProperty("audio_id")]
    [JsonPropertyName("audio_id")]
    public string AudioId { get; set; }
}
