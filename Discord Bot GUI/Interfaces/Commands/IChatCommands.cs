using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Commands
{
    internal interface IChatCommands
    {
        [Command("8ball")]
        public Task Eightball([Remainder] string question);

        [Command("custom list")]
        [RequireContext(ContextType.Guild)]
        public Task CustomList();

        [Command("help")]
        public Task Help();

        [Command("coin flip")]
        [Alias(new string[] { "flip a coin", "flip coin", "flipcoin", "cf", "fc", "cofl", "flco", "coin", "flip", "coinflip", "50/50", "pick" })]
        public Task CoinFlip([Remainder] string message);

        [Command("remind at")]
        [Alias(new string[] { "reminder at" })]
        public Task RemindAt([Remainder] string message);

        [Command("remind in")]
        [Alias(new string[] { "reminder in" })]
        public Task RemindIn([Remainder] string message);

        [Command("remind list")]
        [Alias(new string[] { "reminder list" })]
        public Task RemindList();

        [Command("remind remove")]
        [Alias(new string[] { "reminder remove" })]
        public Task RemindRemove(int index);

        [Command("wotd")]
        [Alias(new string[] { "word of the day" })]
        public Task WotDFunction(string language = "korean");
    }
}
