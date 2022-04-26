using System.Threading.Tasks;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Users;

internal class SetMessengerInviteStatusEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public SetMessengerInviteStatusEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var allowMessengerInvites = packet.PopBoolean();
        session.GetHabbo().AllowMessengerInvites = allowMessengerInvites;
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("UPDATE `users` SET `ignore_invites` = @ignoreInvites WHERE `id` = @habboId LIMIT 1");
        dbClient.AddParameter("habboId", session.GetHabbo().Id);
        dbClient.AddParameter("ignoreInvites", ConvertExtensions.ToStringEnumValue(allowMessengerInvites));
        dbClient.RunQuery();
        return Task.CompletedTask;
    }
}