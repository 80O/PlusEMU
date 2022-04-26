using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class GetModeratorUserChatlogEvent : IPacketEvent
{
    public readonly IChatlogManager _chatlogManager;
    public readonly IDatabase _database;

    public GetModeratorUserChatlogEvent(IChatlogManager chatlogManager, IDatabase database)
    {
        _chatlogManager = chatlogManager; 
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        var data = PlusEnvironment.GetHabboById(packet.PopInt());
        if (data == null)
        {
            session.SendNotification("Unable to load info for user.");
            return Task.CompletedTask;
        }
        _chatlogManager.FlushAndSave();
        var chatlogs = new List<KeyValuePair<RoomData, List<ChatlogEntry>>>();
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("SELECT `room_id`,`entry_timestamp`,`exit_timestamp` FROM `user_roomvisits` WHERE `user_id` = '" + data.Id + "' ORDER BY `entry_timestamp` DESC LIMIT 7");
        var getLogs = dbClient.GetTable();
        if (getLogs != null)
        {
            foreach (DataRow row in getLogs.Rows)
            {
                if (!RoomFactory.TryGetData(Convert.ToInt32(row["room_id"]), out var roomData))
                    continue;
                var timestampExit = Convert.ToDouble(row["exit_timestamp"]) <= 0 ? UnixTimestamp.GetNow() : Convert.ToDouble(row["exit_timestamp"]);
                chatlogs.Add(new KeyValuePair<RoomData, List<ChatlogEntry>>(roomData, GetChatlogs(roomData, Convert.ToDouble(row["entry_timestamp"]), timestampExit)));
            }
        }
        session.SendPacket(new ModeratorUserChatlogComposer(data, chatlogs));
        return Task.CompletedTask;
    }

    private List<ChatlogEntry> GetChatlogs(RoomData roomData, double timeEnter, double timeExit)
    {
        var chats = new List<ChatlogEntry>();
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("SELECT `user_id`, `timestamp`, `message` FROM `chatlogs` WHERE `room_id` = " + roomData.Id + " AND `timestamp` > " + timeEnter + " AND `timestamp` < " + timeExit +
                          " ORDER BY `timestamp` DESC LIMIT 100");
        var data = dbClient.GetTable();
        if (data != null)
        {
            foreach (DataRow row in data.Rows)
            {
                var habbo = PlusEnvironment.GetHabboById(Convert.ToInt32(row["user_id"]));
                if (habbo != null) chats.Add(new ChatlogEntry(Convert.ToInt32(row["user_id"]), roomData.Id, Convert.ToString(row["message"]), Convert.ToDouble(row["timestamp"]), habbo));
            }
        }
        return chats;
    }
}