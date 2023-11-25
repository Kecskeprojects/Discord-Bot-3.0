using Discord.Commands;
using Discord;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using System;
using System.Threading.Tasks;
using Discord_Bot.Interfaces.DBServices;

namespace Discord_Bot.Commands
{
    public class BiasAliasCommands(Logging logger, Config config, IIdolAliasService idolAliasService) : BaseCommand(logger, config), IBiasAliasCommands
    {
        private readonly IIdolAliasService idolAliasService = idolAliasService;

        [Command("bias alias add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Admin command for adding a new bias alias into our lists")]
        public async Task AddBiasAlias([Remainder] string biasData)
        {
            try
            {
                string biasAlias = biasData.ToLower().Split('-')[0].Trim();
                string biasName = biasData.ToLower().Split('-')[1].Trim();
                string biasGroup = biasData.ToLower().Split('-')[2].Trim();

                if (string.IsNullOrEmpty(biasName) || string.IsNullOrEmpty(biasGroup))
                {
                    return;
                }

                DbProcessResultEnum result = await idolAliasService.AddIdolAliasAsync(biasAlias, biasName, biasGroup);
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
                logger.Error("BiasAliasCommands.cs AddBiasAlias", ex.ToString());
            }

        }

        [Command("bias alias remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Admin command for removing a bias alias from our lists")]
        public async Task RemoveBiasAlias([Remainder] string biasData)
        {
            try
            {
                string biasAlias = biasData.ToLower().Split('-')[0].Trim();
                string biasName = biasData.ToLower().Split('-')[1].Trim();
                string biasGroup = biasData.ToLower().Split('-')[2].Trim();

                if (string.IsNullOrEmpty(biasName) || string.IsNullOrEmpty(biasGroup))
                {
                    return;
                }

                DbProcessResultEnum result = await idolAliasService.RemoveIdolAliasAsync(biasAlias, biasName, biasGroup);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Bias alias added to list!");
                }
                else if (result == DbProcessResultEnum.AlreadyExists)
                {
                    await ReplyAsync("Bias alias already in database!");
                }
                else
                {
                    await ReplyAsync("Bias alias could not be added!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("BiasAliasCommands.cs RemoveBiasAlias", ex.ToString());
            }

        }
    }
}
