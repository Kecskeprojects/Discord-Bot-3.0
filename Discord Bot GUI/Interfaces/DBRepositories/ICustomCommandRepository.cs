﻿using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface ICustomCommandRepository
    {
        Task<CustomCommand> GetCustomCommandAsync(ulong id, string commandName);
        Task<List<CustomCommand>> GetCustomCommandAsync(ulong id);
    }
}
