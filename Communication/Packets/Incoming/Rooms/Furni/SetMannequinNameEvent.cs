using System;
using System.Threading.Tasks;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class SetMannequinNameEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public SetMannequinNameEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null || !room.CheckRights(session, true))
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var name = packet.PopString();
        var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        if (item.ExtraData.Contains(Convert.ToChar(5)))
        {
            var flags = item.ExtraData.Split(Convert.ToChar(5));
            item.ExtraData = flags[0] + Convert.ToChar(5) + flags[1] + Convert.ToChar(5) + name;
        }
        else
            item.ExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `items` SET `extra_data` = @Ed WHERE `id` = @itemId LIMIT 1");
            dbClient.AddParameter("itemId", item.Id);
            dbClient.AddParameter("Ed", item.ExtraData);
            dbClient.RunQuery();
        }
        item.UpdateState(true, true);
        return Task.CompletedTask;
    }
}