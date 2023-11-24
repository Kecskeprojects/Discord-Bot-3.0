using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;

namespace Discord_Bot.Commands
{
    public class VoiceCommands(
        IServerService serverService,
        IAudioService audioService,
        Logging logger,
        Config config) : CommandBase(logger, config), IVoiceCommands
    {
        private readonly IServerService serverService = serverService;
        private readonly IAudioService audioService = audioService;

        #region Main request command
        [Command("p")]
        [Alias(["play"])]
        [RequireContext(ContextType.Guild)]
        [Summary("Play music on the channel the user is connected to")]
        public async Task Play([Remainder] string content)
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id))
                {
                    return;
                }

                await audioService.RequestHandler(Context, content);

                if (!Global.ServerAudioResources[sId].AudioVariables.Playing)
                {
                    Global.ServerAudioResources[sId].AudioVariables.Playing = true;
                    Global.ServerAudioResources[sId].AudioVariables.AbruptDisconnect = false;

                    if (Global.ServerAudioResources[sId].MusicRequests.Count > 0)
                    {
                        await audioService.PlayHandler(Context, server, sId);
                    }

                    Global.ServerAudioResources[sId].AudioVariables.Playing = false;

                    Global.ServerAudioResources[sId].AudioVariables = new();
                    Global.ServerAudioResources[sId].MusicRequests.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.Error("VoiceCommands.cs Play", ex.ToString());
            }
        }
        #endregion

        #region Voice connection commands
        [Command("join")]
        [Alias(["connect", "conn"])]
        [RequireContext(ContextType.Guild)]
        [Summary("Joins the bot to the user's voice channel")]
        public async Task Join()
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, sId))
                {
                    return;
                }

                await audioService.ConnectBot(Context, server, sId);
            }
            catch (Exception ex)
            {
                logger.Error("VoiceCommands.cs Join", ex.ToString());
            }
        }

        [Command("leave")]
        [Alias(["disconnect", "disconn", "disc", "dc"])]
        [RequireContext(ContextType.Guild)]
        [Summary("Leaves the current voice channel")]
        public async Task Leave()
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id))
                {
                    return;
                }

                Global.ServerAudioResources[sId].MusicRequests.Clear();

                var clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id);
                await (clientUser as SocketGuildUser).VoiceChannel.DisconnectAsync();

                Global.ServerAudioResources[sId].AudioVariables.FFmpeg.Kill();
                Global.ServerAudioResources[sId].AudioVariables.Output.Dispose();
            }
            catch(Exception ex)
            {
                logger.Error("VoiceCommands.cs Leave", ex.ToString());
            }
        }
        #endregion

        #region Playlist related commands
        [Command("queue")]
        [Alias(["q"])]
        [RequireContext(ContextType.Guild)]
        [Summary("Current music request queue")]
        public async Task Queue(int index = 1)
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id) || Global.ServerAudioResources[sId].MusicRequests.Count == 0)
                {
                    return;
                }

                int songcount = Global.ServerAudioResources[sId].MusicRequests.Count;

                //If queue does not have songs on that page, do not show a queue
                if (index * 10 <= songcount || (index - 1) * 10 < songcount && index * 10 >= songcount)
                {
                    EmbedBuilder builder = VoiceService.CreateQueueEmbed(index, sId, songcount);

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception ex)
            {
                logger.Error("VoiceCommands.cs Queue", ex.ToString());
            }
        }

        [Command("np")]
        [Alias(["now playing", "nowplaying"])]
        [RequireContext(ContextType.Guild)]
        [Summary("The currently playing song")]
        public async Task NowPlaying()
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id) || Global.ServerAudioResources[sId].MusicRequests.Count == 0)
                {
                    return;
                }

                await VoiceService.NpEmbed(Context.Channel, sId);
            }
            catch (Exception ex)
            {
                logger.Error("VoiceCommands.cs NowPlaying", ex.ToString());
            }
        }

        [Command("clear")]
        [RequireContext(ContextType.Guild)]
        [Summary("Clear playlist and leave voice channel")]
        public async Task Clear()
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id) || Global.ServerAudioResources[sId].MusicRequests.Count == 0)
                {
                    return;
                }

                Global.ServerAudioResources[sId].MusicRequests.Clear();

                await Context.Channel.SendMessageAsync("The queue has been cleared!");

                Global.ServerAudioResources[sId].AudioVariables.FFmpeg.Kill();
                Global.ServerAudioResources[sId].AudioVariables.Output.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error("VoiceCommands.cs Clear", ex.ToString());
            }
        }

        [Command("skip")]
        [RequireContext(ContextType.Guild)]
        [Summary("Skip current song")]
        public async Task Skip()
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id) || Global.ServerAudioResources[sId].MusicRequests.Count == 0)
                {
                    return;
                }

                await Context.Channel.SendMessageAsync("Song skipped!");

                Global.ServerAudioResources[sId].AudioVariables.FFmpeg.Kill();
                Global.ServerAudioResources[sId].AudioVariables.Output.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error("VoiceCommands.cs Skip", ex.ToString());
            }
        }

        [Command("remove")]
        [RequireContext(ContextType.Guild)]
        [Alias(["r"])]
        [Summary("Removes the song at the given position")]
        public async Task Remove(int position)
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id) || position < 1 || position >= Global.ServerAudioResources[sId].MusicRequests.Count)
                {
                    return;
                }

                await ReplyAsync($"`{Global.ServerAudioResources[sId].MusicRequests[position].Title}` has been removed from the playlist!");

                Global.ServerAudioResources[sId].MusicRequests.RemoveAt(position);
            }
            catch (Exception ex)
            {
                logger.Error("VoiceCommands.cs Remove", ex.ToString());
            }
        }

        [Command("shuffle")]
        [RequireContext(ContextType.Guild)]
        [Summary("Shuffle the current playlist")]
        public async Task Shuffle()
        {
            try
            {
                ulong sId = Context.Guild.Id;
                ServerResource server = await serverService.GetByDiscordIdAsync(sId);
                if (!Global.IsTypeOfChannel(server, Enums.ChannelTypeEnum.MusicText, Context.Channel.Id) || Global.ServerAudioResources[sId].MusicRequests.Count < 2)
                {
                    return;
                }

                VoiceService.ShufflePlaylist(sId);

                await ReplyAsync("Shuffle complete!");
            }
            catch (Exception ex)
            {
                logger.Error("VoiceCommands.cs Shuffle", ex.ToString());
            }
        }
        #endregion
    }
}
