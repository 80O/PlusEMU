using System.Linq;
using System.Threading.Tasks;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class RemoveBuddyEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IDatabase _database;

    public RemoveBuddyEvent(IGameClientManager clientManager, IDatabase database)
    {
        _clientManager = clientManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var amount = packet.PopInt();
        if (amount > 100)
            amount = 100;
        else if (amount < 0)
            return Task.CompletedTask;
        using var connection = _database.Connection();
        for (var i = 0; i < amount; i++)
        {
            var id = packet.PopInt();
            if (session.GetHabbo().Relationships.Count(x => x.Value.UserId == id) > 0)
            {
                connection.Execute("DELETE FROM `user_relationships` WHERE `user_id` = @id AND `target` = @target OR `target` = @id AND `user_id` = @target",
                    new { id = session.GetHabbo().Id, target  = id });
            }
            if (session.GetHabbo().Relationships.ContainsKey(id))
                session.GetHabbo().Relationships.Remove(id);
            var target = _clientManager.GetClientByUserId(id);
            if (target != null)
            {
                if (target.GetHabbo().Relationships.ContainsKey(session.GetHabbo().Id))
                    target.GetHabbo().Relationships.Remove(session.GetHabbo().Id);
            }
            session.GetHabbo().GetMessenger().DestroyFriendship(id);
        }
        return Task.CompletedTask;
    }
}