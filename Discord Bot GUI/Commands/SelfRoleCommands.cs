using Discord.Commands;
using Discord;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord_Bot.Interfaces.DBServices;

namespace Discord_Bot.Commands
{
    public class SelfRoleCommands : CommandBase, ISelfRoleCommands
    {
        private readonly IRoleService roleService;

        public SelfRoleCommands(IRoleService roleService, Logging logger, Config config) : base(logger, config)
        {
            this.roleService = roleService;
        }

        [Command("self role add")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [Summary("Adding self role to database")]
        public async Task SelfRoleAdd([Remainder] string name)
        {
            try
            {
                //Check if role with that name exists
                IRole role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();

                if (role != null)
                {
                    DbProcessResultEnum result = await roleService.AddSelfRoleAsync(Context.Guild.Id, role.Name.ToLower(), role.Id);
                    if (result == DbProcessResultEnum.Success)
                    {
                        await ReplyAsync($"New role successfully added: {role.Name}");
                    }
                    else if (result == DbProcessResultEnum.AlreadyExists)
                    {
                        await ReplyAsync("Role already in database!");
                    }
                    else
                    {
                        await ReplyAsync("Role could not be added!");
                    }
                }
                else await ReplyAsync("Role not found!");
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs SelfRoleAdd", ex.ToString());
            }
        }

        [Command("self role remove")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [Summary("Removing self role from database")]
        public async Task SelfRoleRemove([Remainder] string name)
        {
            try
            {
                //Check if role with that name exists
                IRole role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();

                if (role != null)
                {
                    DbProcessResultEnum result = await roleService.RemoveSelfRoleAsync(Context.Guild.Id, name);
                    if (result == DbProcessResultEnum.Success)
                    {
                        await ReplyAsync($"The {role.Name} role has been removed.");
                    }
                    else if (result == DbProcessResultEnum.NotFound)
                    {
                        await ReplyAsync("Role could not be found.");
                    }
                    else
                    {
                        await ReplyAsync("Role could not be removed!");
                    }
                }
                else
                {
                    await ReplyAsync("Role does not exist!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("AdminCommands.cs SelfRoleRemove", ex.ToString());
            }
        }
    }
}
