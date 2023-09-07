using System.Collections;

namespace Discord_Bot.Tools
{
    public class CollectionTools
    {
        public static bool IsNullOrEmpty<T>(T array) where T : ICollection
        {
            return array == null || array.Count == 0;
        }
    }
}
