namespace Discord_Bot.Enums
{
    public class BiasEditActionTypeEnum
    {
        public BiasEditActionTypeEnum(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static BiasEditActionTypeEnum EditGroup => new("editgroup");
        public static BiasEditActionTypeEnum EditIdol => new("editidol");
        public static BiasEditActionTypeEnum ChangeGroup => new("changegroup");
        public static BiasEditActionTypeEnum ChangeProfileLink => new("changeprofilelink");
        public static BiasEditActionTypeEnum RemoveImage => new("removeimage");
    }
}
