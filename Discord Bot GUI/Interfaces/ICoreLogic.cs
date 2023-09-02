using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces
{
    public interface ICoreLogic
    {
        Task CustomCommands(SocketCommandContext context);
        Task SelfRole(SocketCommandContext context);
        Task FeatureChecks(SocketCommandContext context);
        Task ReminderCheck(DiscordSocketClient Client);
        void Closing();
        void Closing(object sender, EventArgs e);
        void InstagramEmbed(SocketCommandContext context);
        void TwitterEmbed(SocketCommandContext context);
        void LogToFile();
        void Check_Folders();
    }
}
