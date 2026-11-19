using DatabaseORM.Enums;
using DatabaseORM.Model;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Common.Builders;

//EF entities with the shared test IDs, with their keys and navigations set as a query would return them
public static class Entities
{
    public static Server Server(int serverId = RowIds.Server, ulong discordId = DiscordIds.Server)
    {
        return new Server { ServerId = serverId, DiscordId = discordId.ToString() };
    }

    public static User User(int userId = RowIds.User, ulong discordId = DiscordIds.User)
    {
        return new User { UserId = userId, DiscordId = discordId.ToString() };
    }

    //The server is left unset for the mapping tests, which don't need it
    public static Role Role(ulong discordId, string name, Server? server = null)
    {
        return new Role { DiscordId = discordId.ToString(), RoleName = name, Server = server! };
    }

    public static ChannelType ChannelType(ChannelTypeEnum type)
    {
        return new ChannelType { ChannelTypeId = (int) type, Name = type.ToString() };
    }

    public static Channel Channel(Server server, ulong discordId, params ChannelType[] types)
    {
        return new Channel { ServerId = server.ServerId, Server = server, DiscordId = discordId.ToString(), ChannelTypes = [.. types] };
    }
}
