using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class GetModeratorUserRoomVisitsEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IDatabase _database;

    public GetModeratorUserRoomVisitsEvent(IGameClientManager clientManager, IDatabase database)
    {
        _clientManager = clientManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        var userId = packet.PopInt();
        var target = _clientManager.GetClientByUserId(userId);
        if (target == null)
            return Task.CompletedTask;
        var visits = new Dictionary<double, RoomData>();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `room_id`, `entry_timestamp` FROM `user_roomvisits` WHERE `user_id` = @id ORDER BY `entry_timestamp` DESC LIMIT 50");
            dbClient.AddParameter("id", userId);
            var table = dbClient.GetTable();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (!RoomFactory.TryGetData(Convert.ToInt32(row["room_id"]), out var data))
                        continue;
                    if (!visits.ContainsKey(Convert.ToDouble(row["entry_timestamp"])))
                        visits.Add(Convert.ToDouble(row["entry_timestamp"]), data);
                }
            }
        }
        session.SendPacket(new ModeratorUserRoomVisitsComposer(target.GetHabbo(), visits));
        return Task.CompletedTask;
    }
}