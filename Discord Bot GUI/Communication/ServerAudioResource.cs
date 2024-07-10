using System.Collections.Generic;

namespace Discord_Bot.Communication;

public class ServerAudioResource(ulong serverDiscordId)
{
    public ulong ServerDiscordId { get; set; } = serverDiscordId;
    public AudioVariables AudioVariables { get; set; } = new();
    public List<MusicRequest> MusicRequests { get; set; } = []; //Todo: The prospects of turning this into a Queue should be considered as that better reflects it's purpose
}
