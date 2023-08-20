using Discord_Bot.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Core.Caching
{
    public class CacheType
    {
        private CacheType(string value) { Value = value; }

        public string Value { get; private set; }

        public static CacheType Server { get { return new CacheType("server"); } }
    }
}
