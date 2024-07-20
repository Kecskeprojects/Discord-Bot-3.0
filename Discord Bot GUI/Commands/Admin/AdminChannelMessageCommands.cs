using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;

//Todo: Welcome Message Embed similar to Mee6 bot
//User can send 10 or 9 images as attachments, this limit goes down depending on the number of separate embeds
//User can send json in form of file or as the text message (2000 character limit)
//Possibility to edit/add to/remove from/delete existing embeds
//One embed group per channel
//Command to return the existing embed as json
//Command to give a sample to the creation json, or creation website links, possibly both
//https://embed.dan.onl/
//https://message.style/

namespace Discord_Bot.Commands.Admin;
public class AdminPinnedMessageCommands(
    BotLogger logger,
    Config config,
    IServerService serverService) : BaseCommand(logger, config, serverService)
{
}
