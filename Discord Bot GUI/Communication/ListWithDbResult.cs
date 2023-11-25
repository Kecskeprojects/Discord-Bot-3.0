using Discord_Bot.Enums;
using System.Collections.Generic;

namespace Discord_Bot.Communication
{
    public class ListWithDbResult<T>(List<T> list, DbProcessResultEnum result)
    {
        public List<T> List { get; set; } = list;
        public DbProcessResultEnum ProcessResultEnum { get; set; } = result;
    }
}
