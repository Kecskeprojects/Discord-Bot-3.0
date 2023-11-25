using Discord_Bot.Database.Models;
using System.Collections.Generic;

namespace Discord_Bot.Resources
{
    public class IdolGroupResource
    {
        public int GroupId { get; set; }

        public string Name { get; set; }

        public List<Idol> Idols { get; set; } = [];
    }
}
