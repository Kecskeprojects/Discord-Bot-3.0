namespace Discord_Bot.Tools
{
    public class StringTools
    {
        public static string AddNumberPositionIdentifier(string position)
        {
            switch (position[^1])
            {
                case '1': { position += "st"; break; }
                case '2': { position += "nd"; break; }
                case '3': { position += "rd"; break; }
                default: { position += "th"; break; }
            }

            return position;
        }
    }
}
