﻿using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface ICustomCommandService
    {
        Task<CustomCommandResource> GetCustomCommandAsync(ulong id, string commandName);
    }
}