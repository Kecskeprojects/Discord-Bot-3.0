using System.Collections.Generic;

namespace Discord_Bot.Communication
{
    public class ServerAudioResource
    {
        public ServerAudioResource(ulong serverDiscordId)
        {
            ServerDiscordId = serverDiscordId;
            AudioVariables = new();
            MusicRequests = new();
        }

        public ulong ServerDiscordId { get; set; }
        public AudioVariables AudioVariables { get; set; }
        public List<MusicRequest> MusicRequests { get; set; }
    }
}
