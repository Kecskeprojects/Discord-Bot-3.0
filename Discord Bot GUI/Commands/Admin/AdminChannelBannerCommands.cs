using Discord.Commands;
using Discord;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using System.Threading.Tasks;
using System;

//Todo: Welcome Message Embed similar to Mee6 bot
//User can send 10 or 9 images as attachments, this limit goes down depending on the number of separate embeds
//User can send json in form of file or as the text message (2000 character limit)
//Possibility to edit/add to/remove from/delete existing embeds
//One embed group per channel
//Command to return the existing embed as json
//Command to give a sample to the creation json, or creation website links, possibly both
//https://embed.dan.onl/
//https://message.style/
//Todo: Add MessageId to Embed Table too
//Questions:
//Images have to be separate otherwise the Collage logic of discord will kick in
//This means every embed in theory could be a separate message, or at least, blocks separated by images
//None of the editing sites completely support the way this will work, but a compromise can be made (Images are empty embeds with just the image url field having data)
//Walls of text and images however should not need a json, rather an order number should suffice in deciding where they are placed in the order

namespace Discord_Bot.Commands.Admin;
public class AdminChannelBannerCommands(
    BotLogger logger,
    Config config,
    IServerService serverService) : BaseCommand(logger, config, serverService)
{
    [Command("banner add")]
    [Alias(["message add", "banner message add"])]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [Summary("Add to existing or create new banner message")]
    public async Task BannerAdd(string channelName, int order = 0, [Remainder] string content = "")
    {
        try
        {
            if(order == 0)
            {
                //add it to the end of the current mesages
            }

            //content could be part of wall of text that is not embed or image url
            //Additionally, Attachments will be checked, images and Json files, but only one json file
            //If one with the order number is not found, return a list of current order numbers with message links if possible
            await ReplyAsync("BannerAdd Placeholder.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminChannelBannerCommands.cs BannerAdd", ex);
        }
    }

    [Command("banner remove")]
    [Alias(["message remove", "banner message remove"])]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [Summary("Remove from existing banner message")]
    public async Task BannerRemove(string channelName, int order = 0)
    {
        try
        {
            if (order == 0)
            {
                //Remove whole message group
            }
            //If one with the order number is not found, return a list of current order numbers with message links if possible
            await ReplyAsync("BannerEdit Placeholder.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminChannelBannerCommands.cs BannerRemove", ex);
        }
    }

    [Command("banner edit")]
    [Alias(["message edit", "banner message edit"])]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [Summary("Edit existing banner message")]
    public async Task BannerEdit(string channelName, int order = 0, [Remainder] string content = "")
    {
        try
        {
            if (order == 0)
            {
                //This option only works with an attached Json
            }
            //If an order is defined, similar to Add, this will accept image links and walls of text
            //Attachments can be images and Json files
            //If one with the order number is not found, return a list of current order numbers with message links if possible
            await ReplyAsync("BannerEdit Placeholder.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminChannelBannerCommands.cs BannerEdit", ex);
        }
    }

    [Command("banner get")]
    [Alias(["message get", "banner message get"])]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [Summary("Get existing banner message")]
    public async Task BannerGet(string channelName, int order = 0)
    {
        try
        {
            if (order == 0)
            {
                //This option will give you back all the current messages as a Json
            }
            //If an order is defined, this will return the given message
            //If one with the order number is not found, return a list of current order numbers with message links if possible
            await ReplyAsync("BannerGet Placeholder.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminChannelBannerCommands.cs BannerGet", ex);
        }
    }

    [Command("banner order")]
    [Alias(["message order", "banner message order"])]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [Summary("Get existing banner message order")]
    public async Task BannerOrder(string channelName)
    {
        try
        {
            //Return a list of current messages in channel with order numbers, and with links to it, if possible
            await ReplyAsync("BannerOrder Placeholder.");
        }
        catch (Exception ex)
        {
            logger.Error("AdminChannelBannerCommands.cs BannerOrder", ex);
        }
    }
}
