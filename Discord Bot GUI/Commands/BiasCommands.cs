using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.CommandsService;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class BiasCommands(Logging logger, Config config, IIdolService biasService) : BaseCommand(logger, config), IBiasCommands
    {
        private readonly IIdolService idolService = biasService;

        #region Admin permission bias commands
        [Command("biaslist add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Admin command for adding a new bias into our lists")]
        public async Task AddBiasList([Remainder] string biasData)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasData = biasData.ToLower().Trim();

                string biasName = biasData.Split('-')[0];
                string biasGroup = biasData.Split('-')[1];

                DbProcessResultEnum result = await idolService.AddBiasAsync(biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias added to list!");
                }
                else if (result == DbProcessResultEnum.AlreadyExists)
                {
                    await ReplyAsync("Bias already in database!");
                }
                else
                {
                    await ReplyAsync("Bias could not be added!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasCommands.cs AddBiasList", ex.ToString());
            }

        }

        [Command("biaslist remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Admin command for removing a bias from our lists")]
        public async Task RemoveBiasList([Remainder] string biasData)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasData = biasData.ToLower().Trim();

                string biasName = biasData.Split('-')[0];
                string biasGroup = biasData.Split('-')[1];

                //Try removing them from the database
                DbProcessResultEnum result = await idolService.RemoveBiasAsync(biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias removed from list!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Bias could not be found.");
                }
                else
                {
                    await ReplyAsync("Bias could not be removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasCommands.cs RemoveBiasList", ex.ToString());
            }
        }
        #endregion

        #region Bias management commands
        [Command("bias add")]
        [Summary("Command for adding a new bias into a user's list")]
        public async Task AddBias([Remainder] string biasData)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                string biasName = "";
                string biasGroup = "";

                if (biasData.Contains('-'))
                {
                    biasName = biasData.ToLower().Split('-')[0].Trim();
                    biasGroup = biasData.ToLower().Split('-')[1].Trim();
                }
                else
                {
                    biasName = biasData.ToLower().Trim();
                }

                DbProcessResultEnum result = await idolService.AddUserBiasAsync(Context.User.Id, biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias added to your list of biases!");
                }
                else if (result == DbProcessResultEnum.MultipleResults)
                {
                    await ReplyAsync("There are multiple biases with that name!\nWrite down the group name too in the following format: [name]-[group]");
                }
                else if (result == DbProcessResultEnum.AlreadyExists)
                {
                    await ReplyAsync("You already have this bias on your list!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Bias not in database!");
                }
                else
                {
                    await ReplyAsync("Bias could not be added!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasCommands.cs AddBias", ex.ToString());
            }
        }

        [Command("bias remove")]
        [Summary("Command for removing a bias from a user's list")]
        public async Task RemoveBias([Remainder] string biasData)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                string biasName = "";
                string biasGroup = "";

                if (biasData.Contains('-'))
                {
                    biasName = biasData.ToLower().Split('-')[0].Trim();
                    biasGroup = biasData.ToLower().Split('-')[1].Trim();
                }
                else
                {
                    biasName = biasData.ToLower().Trim();
                }

                DbProcessResultEnum result = await idolService.AddUserBiasAsync(Context.User.Id, biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias removed from your list of biases!");
                }
                else if (result == DbProcessResultEnum.MultipleResults)
                {
                    await ReplyAsync("You have multiple biases with that name!\nWrite down the group name too in the following format: [name]-[group]");
                }
                else if (result == DbProcessResultEnum.PartialNotFound)
                {
                    await ReplyAsync("You do not have this bias on your list!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Bias not in database!");
                }
                else
                {
                    await ReplyAsync("Bias could not be removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasCommands.cs RemoveBias", ex.ToString());
            }
        }

        [Command("bias clear")]
        [Summary("Command for clearing the user's bias list")]
        public async Task ClearBias()
        {
            try
            {
                DbProcessResultEnum result = await idolService.ClearUserBiasAsync(Context.User.Id);
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
                logger.Error("BiasCommands.cs ClearBias", ex.ToString());
            }
        }

        [Command("my biases")]
        [Alias(["mybiases", "biases", "my bias"])]
        [Summary("Command to check a user's current list of biases")]
        public async Task MyBiases([Remainder] string groupName = "")
        {
            try
            {
                groupName = groupName.ToLower();

                //Get your list of biases
                List<IdolResource> list = idolService.UserBiasesListAsync(Context.User.Id, groupName);

                //Check if you have any
                if (list == null)
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
                logger.Error("BiasCommands.cs MyBiases", ex.ToString());
            }
        }
        #endregion

        #region List biases command
        [Command("bias list")]
        [Summary("Command for checking our biases as a whole")]
        public async Task BiasList([Remainder] string groupName = "")
        {
            try
            {
                //Get the global list of biases
                List<IdolResource> idols = await idolService.GetBiasesByGroupAsync(groupName.ToLower());

                //Check if we have any items on the list
                if (idols.Count == 0)
                {
                    if (groupName != "") { await ReplyAsync("No biases from that group are in the database!"); }
                    else { await ReplyAsync("No biases have been added yet!"); }
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
                logger.Error("BiasCommands.cs BiasList", ex.ToString());
            }
        }
        #endregion

        #region Ping command
        [Command("ping")]
        [RequireContext(ContextType.Guild)]
        [Summary("Command for pinging biases")]
        public async Task PingBias([Remainder] string biasNames)
        {
            try
            {
                //Make the name lowercase and split up names
                biasNames = biasNames.ToLower();
                string[] nameList = biasNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                //Get the users that have the bias with the following names, if the bias exists
                ListWithDbResult<UserResource> result = idolService.GetUsersWithBiasesAsync(nameList);

                if (result.ProcessResultEnum == Enums.DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("No bias found with that name/those names!");
                }
                else if (result.ProcessResultEnum == Enums.DbProcessResultEnum.Failure || result.List == null)
                {
                    await ReplyAsync("Exception during search for users!");
                }
                else
                {
                    //If only one person has the bias and it's the command sender, send unique message
                    if (result.List.Count == 0)
                    {
                        await ReplyAsync("Not even you have this bias, that's just shameful really...");
                        return;
                    }
                    else if (result.List.Count == 1 && result.List[0].DiscordId == Context.User.Id)
                    {
                        await ReplyAsync("Only you have that bias for now! Time to convert people.");
                        return;
                    }

                    await Context.Guild.DownloadUsersAsync();

                    string message = "";
                    //Make a list of mentions out of them
                    foreach (UserResource dbUser in result.List)
                    {
                        //Find user on server
                        SocketGuildUser user = Context.Guild.GetUser(dbUser.DiscordId);

                        //If user is not found or the user is the one sending the command, do not add their mention to the list
                        if (user != null && user.Id != Context.User.Id)
                        {
                            message += user.Mention + " ";
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //delete command and send mentions
                    await Context.Message.DeleteAsync();
                    await ReplyAsync(message);
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasCommands.cs PingBias", ex.ToString());
            }
        }
        #endregion
    }
}
