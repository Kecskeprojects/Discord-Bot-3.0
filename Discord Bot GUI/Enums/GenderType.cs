namespace Discord_Bot.Enums
{
    public class GenderType
    {
        private GenderType(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static GenderType Male => new("M");
        public static GenderType Female => new("F");
    }
}
