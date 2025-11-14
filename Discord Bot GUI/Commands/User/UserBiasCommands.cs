using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Resources;
using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Bias")]
[Remarks("User")]
[Summary("User bias related commands")]
public class UserBiasCommands(
    IUserIdolService userIdolService,
    IIdolService idolService,
    IIdolGroupService idolGroupService,
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    private readonly IUserIdolService userIdolService = userIdolService;
    private readonly IIdolService idolService = idolService;
    private readonly IIdolGroupService idolGroupService = idolGroupService;

    [Command("bias add")]
    [Summary("Adding a new bias to your list, the group is optional but may be needed if multiple idols of the same stage name exist")]
    public async Task AddBias([Name("stage name-group")][Remainder] string parameters)
    {
        try
        {
            string[] paramArray = GetParametersBySplit(parameters, '-');

            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            //Make the name lowercase and clear and accidental spaces
            string biasName = "";
            string biasGroup = "";

            bool isGroupName = false;
            if (paramArray.Length == 2)
            {
                biasName = paramArray[0];
                biasGroup = paramArray[1];
            }
            else
            {
                isGroupName = await idolGroupService.GroupExistsAsnyc(parameters.ToLower().Trim());
                if (isGroupName)
                {
                    biasGroup = parameters.ToLower().Trim();
                }
                else
                {
                    biasName = parameters.ToLower().Trim();
                }
            }

            DbProcessResultEnum result = isGroupName
                ? await userIdolService.AddUserIdolGroupAsync(Context.User.Id, biasGroup)
                : await userIdolService.AddUserIdolAsync(Context.User.Id, biasName, biasGroup);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Bias(es) added to your list of biases.",
                DbProcessResultEnum.MultipleResults => "There are multiple biases with that name.\nWrite down the group name too in the following format: [name]-[group].",
                DbProcessResultEnum.AlreadyExists => "You already have this bias/these biases on your list.",
                DbProcessResultEnum.MultipleExists => "A bias with that name was found in your list, but you did not specify a group. (format: [name]-[group])",
                DbProcessResultEnum.NotFound => "Bias not in database.",
                _ => "Bias(es) could not be added!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasCommands.cs AddBias", ex);
        }
    }

    [Command("bias remove")]
    [Summary("Removing a new bias from your list, the group is optional but may be needed if multiple idols of the same stage name exist")]
    public async Task RemoveBias([Name("stage name-group")][Remainder] string parameters)
    {
        try
        {
            string[] paramArray = GetParametersBySplit(parameters, '-');

            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            //Make the name lowercase and clear and accidental spaces
            string biasName = "";
            string biasGroup = "";

            bool isGroupName = false;
            if (paramArray.Length == 2)
            {
                biasName = paramArray[0];
                biasGroup = paramArray[1];
            }
            else
            {
                isGroupName = await idolGroupService.GroupExistsAsnyc(parameters.ToLower().Trim());
                if (isGroupName)
                {
                    biasGroup = parameters.ToLower().Trim();
                }
                else
                {
                    biasName = parameters.ToLower().Trim();
                }
            }

            DbProcessResultEnum result = isGroupName
                ? await userIdolService.RemoveUserIdolGroupAsync(Context.User.Id, biasGroup)
                : await userIdolService.RemoveUserIdolAsync(Context.User.Id, biasName, biasGroup);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Bias(es) removed from your list of biases.",
                DbProcessResultEnum.MultipleResults => "You have multiple biases with that name.\nWrite down the group name too in the following format: [name]-[group].",
                DbProcessResultEnum.PartialNotFound => "You do not have this bias/these biases on your list.",
                _ => "Bias(es) could not be removed!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasCommands.cs RemoveBias", ex);
        }
    }

    [Command("bias clear")]
    [Summary("Remove all your current biases, this action cannot be reversed")]
    public async Task ClearBias()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            DbProcessResultEnum result = await userIdolService.ClearUserIdolAsync(Context.User.Id);
            string resultMessage = result switch
            {
                DbProcessResultEnum.Success => "Your biases have been cleared.",
                _ => "You did not have any biases to clear!"
            };
            _ = await ReplyAsync(resultMessage);
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasCommands.cs ClearBias", ex);
        }
    }

    [Command("my biases")]
    [Alias(["mybiases", "biases", "my bias"])]
    [Summary("Check your current list of biases, if a group name is given, only that group's members are shown")]
    public async Task MyBiases([Name("group")][Remainder] string groupName = "")
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            //Get your list of biases
            List<IdolResource> list = await userIdolService.GetUserIdolsListAsync(Context.User.Id, groupName.ToLower().Trim());

            //Check if you have any
            if (CollectionTools.IsNullOrEmpty(list))
            {
                _ = groupName != ""
                    ? await ReplyAsync("No biases from that group are in your list!")
                    : await ReplyAsync("You do not have any biases set yet!");

                return;
            }

            BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage(list, groupName, $"{GetCurrentUserNickname()}'s biases by group", Context.User.Id, true);
            _ = result.Component == null ? await ReplyAsync(result.Message) : await ReplyAsync(result.Message, components: result.Component);

            //Generate a random number, 10% chance for an additional message to appear
            Random r = new();
            if (r.Next(0, 10) == 0)
            {
                //Pick a random bias
                string bias = list[r.Next(0, list.Count)].Name;
                //Also make the first letter upper case
                bias = bias.ToUpper();

                string baseMessage = Constant.BiasExtraMessage[r.Next(0, Constant.BiasExtraMessage.Length)];
                string message = string.Format(baseMessage, bias);

                _ = await ReplyAsync(message);
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasCommands.cs MyBiases", ex);
        }
    }

    [Command("bias list")]
    [Summary("Command to check all current idols in the database, if a group name is given, only that group's members are shown")]
    public async Task BiasList([Name("group")][Remainder] string groupName = "")
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            //Get the global list of biases
            List<IdolResource> idols = await idolService.GetIdolsByGroupAsync(groupName.ToLower().Trim());

            //Check if we have any items on the list
            if (idols.Count == 0)
            {
                _ = groupName != ""
                    ? await ReplyAsync("No biases from that group are in the database!")
                    : await ReplyAsync("No biases have been added to database yet!");
                return;
            }

            BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage(idols, groupName, "Which group do you want to see?", Context.User.Id, false);
            _ = result.Component == null ? await ReplyAsync(result.Message) : await ReplyAsync(result.Message, components: result.Component);
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasCommands.cs BiasList", ex);
        }
    }

    [Command("ping")]
    [RequireContext(ContextType.Guild)]
    [Summary("Command for pinging people with a given bias\n*Can also be group name, leading to all people getting pinged who have a bias in the group")]
    public async Task PingBias([Name("stage name, stage name,...*")][Remainder] string biasNames)
    {
        try
        {
            string[] nameList = GetParametersBySplit(biasNames, ',');

            //Get the users that have the bias with the following names, if the bias exists
            ListWithDbResult<UserResource> result = await idolService.GetUsersByIdolsAsync(nameList);

            if (result.ProcessResultEnum == DbProcessResultEnum.NotFound)
            {
                _ = await ReplyAsync("No bias found with that name/those names!");
                return;
            }
            else if (result.ProcessResultEnum == DbProcessResultEnum.Failure || result.List == null)
            {
                _ = await ReplyAsync("Exception during search for users!");
                return;
            }
            else
            {
                await Context.Guild.DownloadUsersAsync();

                List<SocketGuildUser> guildUsers = [];
                bool isUsersBias = false;
                foreach (UserResource dbUser in result.List)
                {
                    //Find user on server
                    SocketGuildUser user = Context.Guild.GetUser(dbUser.DiscordId);

                    //If user is not found or the user is the one sending the command, do not add their mention to the list
                    if (user != null)
                    {
                        if (user.Id != Context.User.Id)
                        {
                            guildUsers.Add(user);
                            continue;
                        }
                        isUsersBias = true;
                    }
                }

                //If only one person has the bias and it's the command sender, send unique message
                if (guildUsers.Count == 0)
                {
                    _ = await ReplyAsync(isUsersBias ?
                                    "Only you have that bias for now! Time to convert people." :
                                    "Not even you have this bias, that's just shameful really...");
                    return;
                }

                //Make a list of mentions out of them
                string message = string.Join(" ", guildUsers.Select(u => u.Mention));

                //delete command and send mentions
                await Context.Message.DeleteAsync();
                _ = await ReplyAsync(message);
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasCommands.cs PingBias", ex);
        }
    }
}
