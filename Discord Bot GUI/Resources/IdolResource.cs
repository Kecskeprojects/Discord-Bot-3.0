using System.Collections.Generic;

namespace Discord_Bot.Resources
{
    public class IdolResource
    {
        public int IdolId { get; set; }

        public string Name { get; set; }

        public string GroupName { get; set; }

        public List<IdolAliasResource> IdolAliases { get; set; } = [];
    }
}
