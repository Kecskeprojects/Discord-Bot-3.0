﻿using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IGreetingService
{
    Task<List<GreetingResource>> GetAllGreetingAsync();
    Task<DbProcessResultEnum> AddGreetingAsync(string url);
    Task<DbProcessResultEnum> RemoveGreetingAsync(int id);
}
