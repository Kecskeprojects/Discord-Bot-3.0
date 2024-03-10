namespace Discord_Bot.Communication
{
    public class BiasEditData
    {
        public BiasEditData(string data)
        {
            string[] parts = data.Split(';');

            Action = parts[0];
            BiasName = parts[1];
            BiasGroupName = parts[2];
        }

        public string Action { get; private set; }
        public string BiasName { get; private set; }
        public string BiasGroupName { get; private set; }
    }
}
