using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface ICustomCommandCommands
    {
        Task CustomCommandAdd(string name, string link);
        Task CustomCommandRemove(string name);
    }
}
