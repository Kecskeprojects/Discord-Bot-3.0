using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IReminderService
    {
        Task<List<ReminderResource>> GetCurrentRemindersAsync(DateTime dateTime);
        Task RemoveCurrentRemindersAsync(List<int> reminderIds);
    }
}
