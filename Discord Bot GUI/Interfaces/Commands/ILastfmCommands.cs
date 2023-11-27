using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces
{
    public interface ILastfmCommands
    {
        public Task LfConnect(string name);
        public Task LfDisconnect();
        public Task LfTopTrack(params string[] parameters);
        public Task LfTopAlbum(params string[] parameters);
        public Task LfTopArtist(params string[] parameters);
        public Task LfNowPlaying();
        public Task LfRecent(int limit = 10);
        public Task LfArtist([Remainder] string artist);
        public Task LfWhoKnows([Remainder] string input = "");
    }
}
