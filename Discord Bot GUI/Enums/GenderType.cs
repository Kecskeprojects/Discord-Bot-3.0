namespace Discord_Bot.Enums
{
    public class GenderType
    {
        public static implicit operator string(GenderType en)
        {
            return en.Value;
        }

        public override string ToString() { return Value; }

        private GenderType(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static GenderType Male => new("M");
        public static GenderType Female => new("F");
    }
}
