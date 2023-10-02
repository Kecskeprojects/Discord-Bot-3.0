using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    public interface IKeywordCommands
    {
        Task KeywordAdd([Remainder] string keyword_response);
        Task KeywordRemove(string keyword);
    }
}
