using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IGreetingCommands
    {
        public Task GreetingList();
        public Task GreetingAdd(string url);
        public Task GreetingRemove(int id);
    }
}
