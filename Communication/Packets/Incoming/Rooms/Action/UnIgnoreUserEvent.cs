using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Action;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class UnIgnoreUserEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public UnIgnoreUserEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var username = packet.PopString();
        var player = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username)?.GetHabbo();
        if (player == null)
            return Task.CompletedTask;
        if (!session.GetHabbo().GetIgnores().TryGet(player.Id))
            return Task.CompletedTask;
        if (session.GetHabbo().GetIgnores().TryRemove(player.Id))
        {
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `user_ignores` WHERE `user_id` = @uid AND `ignore_id` = @ignoreId");
                dbClient.AddParameter("uid", session.GetHabbo().Id);
                dbClient.AddParameter("ignoreId", player.Id);
                dbClient.RunQuery();
            }
            session.SendPacket(new IgnoreStatusComposer(3, player.Username));
        }
        return Task.CompletedTask;
    }
}