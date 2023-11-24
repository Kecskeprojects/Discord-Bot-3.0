using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IVoiceCommands
    {
        public Task Play([Remainder] string content);
        public Task Join();
        public Task Leave();
        public Task Queue(int index);
        public Task NowPlaying();
        public Task Clear();
        public Task Skip();
        public Task Remove(int position);
        public Task Shuffle();
    }
}
