namespace Discord_Bot.Enums
{
    public class BiasEditActionTypeEnum(string value)
    {
        public static implicit operator string(BiasEditActionTypeEnum en)
        {
            return en.Value;
        }

        public override string ToString() { return Value; }

        public string Value { get; private set; } = value;

        public static BiasEditActionTypeEnum EditGroup => new("editgroup");
        public static BiasEditActionTypeEnum EditIdol => new("editidol");
        public static BiasEditActionTypeEnum ChangeGroup => new("changegroup");
        public static BiasEditActionTypeEnum ChangeProfileLink => new("changeprofilelink");
        public static BiasEditActionTypeEnum RemoveImage => new("removeimage");
        public static BiasEditActionTypeEnum EditIdolExtended => new("editidolextended");
    }
}
