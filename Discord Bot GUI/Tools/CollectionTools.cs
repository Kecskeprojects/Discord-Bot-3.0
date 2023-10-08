using System.Collections;

namespace Discord_Bot.Tools
{
    public class CollectionTools
    {
        public static bool IsNullOrEmpty<T>(T collection) where T : ICollection
        {
            return collection == null || collection.Count == 0;
        }
    }
}
