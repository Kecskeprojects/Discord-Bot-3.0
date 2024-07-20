using Discord_Bot.Enums;

namespace Discord_Bot.Communication.Bias;
public class BiasEditData
{
    public BiasEditData(string data)
    {
        string[] parts = data.Split(';');

        Action = (BiasEditActionTypeEnum) int.Parse(parts[0]);
        BiasName = parts[1];
        BiasGroupName = parts[2];
    }

    public BiasEditActionTypeEnum Action { get; private set; }
    public string BiasName { get; private set; }
    public string BiasGroupName { get; private set; }
}
