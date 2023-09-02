using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class TwitchChannelService : BaseService, ITwitchChannelService
    {
        private readonly ITwitchChannelRepository twitchChannelRepository;

        public TwitchChannelService(IMapper mapper, Logging logger, Cache cache, ITwitchChannelRepository twitchChannelRepository) : base(mapper, logger, cache) => this.twitchChannelRepository = twitchChannelRepository;

        public async Task<List<TwitchChannelResource>> GetChannelsAsync()
        {
            List<TwitchChannelResource> result = null;
            try
            {
                List<TwitchChannel> channels = await twitchChannelRepository.GetChannelsAsync();
                if (channels == null) return null;

                result = mapper.Map<List<TwitchChannel>, List<TwitchChannelResource>>(channels);
            }
            catch (Exception ex)
            {
                logger.Error("TwitchChannelService.cs GetChannelsAsync", ex.ToString());
            }

            return result;
        }
    }
}
