using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.CommandsService;
using Discord_Bot.Communication;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User
{
    public class UserBiasCommands(
        IUserIdolService userIdolService,
        IIdolService idolService,
        IIdolGroupService idolGroupService,
        IServerService serverService,
        Logging logger,
        Config config) : BaseCommand(logger, config, serverService)
    {
        private readonly IUserIdolService userIdolService = userIdolService;
        private readonly IIdolService idolService = idolService;
        private readonly IIdolGroupService idolGroupService = idolGroupService;

        [Command("bias add")]
        [Summary("Command for adding a new bias into a user's list")]
        public async Task AddBias([Remainder] string biasData)
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await GetCurrentServerAsync();
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                //Make the name lowercase and clear and accidental spaces
                string biasName = "";
                string biasGroup = "";

                bool isGroupName = false;
                if (biasData.Contains('-'))
                {
                    biasName = biasData.ToLower().Split('-')[0].Trim();
                    biasGroup = biasData.ToLower().Split('-')[1].Trim();
                }
                else
                {
                    isGroupName = await idolGroupService.GroupExistsAsnyc(biasData.ToLower().Trim());
                    if (isGroupName)
                    {
                        biasGroup = biasData.ToLower().Trim();
                    }
                    else
                    {
                        biasName = biasData.ToLower().Trim();
                    }
                }

                DbProcessResultEnum result = isGroupName
                    ? await userIdolService.AddUserIdolGroupAsync(Context.User.Id, biasGroup)
                    : await userIdolService.AddUserIdolAsync(Context.User.Id, biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias(es) added to your list of biases!");
                }
                else if (result == DbProcessResultEnum.MultipleResults)
                {
                    await ReplyAsync("There are multiple biases with that name!\nWrite down the group name too in the following format: [name]-[group]");
                }
                else if (result == DbProcessResultEnum.AlreadyExists)
                {
                    await ReplyAsync("You already have this bias/these biases on your list!");
                }
                else if (result == DbProcessResultEnum.MultipleExists)
                {
                    await ReplyAsync("A bias with that name was found in your list, but you did not specify a group! (format: [name]-[group])");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Bias not in database!");
                }
                else
                {
                    await ReplyAsync("Bias(es) could not be added!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("UserBiasCommands.cs AddBias", ex);
            }
        }

        [Command("bias remove")]
        [Summary("Command for removing a bias from a user's list")]
        public async Task RemoveBias([Remainder] string biasData)
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await GetCurrentServerAsync();
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                //Make the name lowercase and clear and accidental spaces
                string biasName = "";
                string biasGroup = "";

                bool isGroupName = false;
                if (biasData.Contains('-'))
                {
                    biasName = biasData.ToLower().Split('-')[0].Trim();
                    biasGroup = biasData.ToLower().Split('-')[1].Trim();
                }
                else
                {
                    isGroupName = await idolGroupService.GroupExistsAsnyc(biasData.ToLower().Trim());
                    if (isGroupName)
                    {
                        biasGroup = biasData.ToLower().Trim();
                    }
                    else
                    {
                        biasName = biasData.ToLower().Trim();
                    }
                }

                DbProcessResultEnum result = isGroupName
                    ? await userIdolService.RemoveUserIdolGroupAsync(Context.User.Id, biasGroup)
                    : await userIdolService.RemoveUserIdolAsync(Context.User.Id, biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias(es) removed from your list of biases!");
                }
                else if (result == DbProcessResultEnum.MultipleResults)
                {
                    await ReplyAsync("You have multiple biases with that name!\nWrite down the group name too in the following format: [name]-[group]");
                }
                else if (result == DbProcessResultEnum.PartialNotFound)
                {
                    await ReplyAsync("You do not have this bias/these biases on your list!");
                }
                else
                {
                    await ReplyAsync("Bias(es) could not be removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("UserBiasCommands.cs RemoveBias", ex);
            }
        }

        [Command("bias clear")]
        [Summary("Command for clearing the user's bias list")]
        public async Task ClearBias()
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await GetCurrentServerAsync();
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                DbProcessResultEnum result = await userIdolService.ClearUserIdolAsync(Context.User.Id);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Your biases have been cleared!");
                }
                else
                {
                    await ReplyAsync("You did not have any biases to clear!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("UserBiasCommands.cs ClearBias", ex);
            }
        }

        [Command("my biases")]
        [Alias(["mybiases", "biases", "my bias"])]
        [Summary("Command to check a user's current list of biases")]
        public async Task MyBiases([Remainder] string groupName = "")
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await GetCurrentServerAsync();
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                //Get your list of biases
                List<IdolResource> list = await userIdolService.GetUserIdolsListAsync(Context.User.Id, groupName.ToLower().Trim());

                //Check if you have any
                if (CollectionTools.IsNullOrEmpty(list))
                {
                    if (groupName != "")
                    {
                        await ReplyAsync("No biases from that group are in your list!");
                    }
                    else
                    {
                        await ReplyAsync("You do not have any biases set yet!");
                    }

                    return;
                }

                BiasMessageResult result = BiasService.BuildBiasMessage(list, groupName, $"{Global.GetNickName(Context.Channel, Context.User)}'s biases by group", Context.User.Id, true);
                if (result.Builder == null)
                {
                    await ReplyAsync(result.Message);
                }
                else
                {
                    await ReplyAsync(result.Message, components: result.Builder.Build());
                }

                //Generate a random number, 10% chance for an additional message to appear
                Random r = new();
                if (r.Next(0, 100) < 10)
                {
                    //Pick a random bias
                    string bias = list[r.Next(0, list.Count)].Name;
                    //Also make the first letter upper case
                    bias = bias.ToUpper();

                    //Choose 1 out of 4 responses
                    switch (r.Next(0, 4))
                    {
                        case 0: { await ReplyAsync($"Between you and me, I quite like {bias} too."); break; }
                        case 1: { await ReplyAsync($"{bias}? Good choice!"); break; }
                        case 2: { await ReplyAsync($"Hmm, this list is quite short, someone with a life over here..."); break; }
                        case 3: { await ReplyAsync($"Oh, {bias} is honestly just great."); break; }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("UserBiasCommands.cs MyBiases", ex);
            }
        }

        [Command("bias list")]
        [Summary("Command for checking our biases as a whole")]
        public async Task BiasList([Remainder] string groupName = "")
        {
            try
            {
                if (Context.Channel.GetChannelType() != ChannelType.DM)
                {
                    ServerResource server = await GetCurrentServerAsync();
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                //Get the global list of biases
                List<IdolResource> idols = await idolService.GetIdolsByGroupAsync(groupName.ToLower().Trim());

                //Check if we have any items on the list
                if (idols.Count == 0)
                {
                    if (groupName != "") { await ReplyAsync("No biases from that group are in the database!"); }
                    else { await ReplyAsync("No biases have been added to database yet!"); }
                    return;
                }

                BiasMessageResult result = BiasService.BuildBiasMessage(idols, groupName, "Which group do you want to see?", Context.User.Id, false);
                if (result.Builder == null)
                {
                    await ReplyAsync(result.Message);
                }
                else
                {
                    await ReplyAsync(result.Message, components: result.Builder.Build());
                }
            }
            catch (Exception ex)
            {
                logger.Error("UserBiasCommands.cs BiasList", ex);
            }
        }

        [Command("ping")]
        [RequireContext(ContextType.Guild)]
        [Summary("Command for pinging biases")]
        public async Task PingBias([Remainder] string biasNames)
        {
            try
            {
                //Make the name lowercase and split up names
                string[] nameList = biasNames.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                //Get the users that have the bias with the following names, if the bias exists
                ListWithDbResult<UserResource> result = await idolService.GetUsersByIdolsAsync(nameList);

                if (result.ProcessResultEnum == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("No bias found with that name/those names!");
                    return;
                }
                else if (result.ProcessResultEnum == DbProcessResultEnum.Failure || result.List == null)
                {
                    await ReplyAsync("Exception during search for users!");
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
                        await ReplyAsync(isUsersBias ?
                                        "Only you have that bias for now! Time to convert people." :
                                        "Not even you have this bias, that's just shameful really...");
                        return;
                    }

                    //Make a list of mentions out of them
                    string message = string.Join(" ", guildUsers.Select(u => u.Mention));

                    //delete command and send mentions
                    await Context.Message.DeleteAsync();
                    await ReplyAsync(message);
                }
            }
            catch (Exception ex)
            {
                logger.Error("UserBiasCommands.cs PingBias", ex);
            }
        }
    }
}
