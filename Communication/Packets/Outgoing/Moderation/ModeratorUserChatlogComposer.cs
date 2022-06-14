using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class ModeratorUserChatlogComposer : ServerPacket
{
    public ModeratorUserChatlogComposer(Habbo habbo, List<KeyValuePair<RoomData, List<ChatlogEntry>>> chatlogs)
        : base(ServerPacketHeader.ModeratorUserChatlogMessageComposer)
    {
        WriteInteger(habbo.Id);
        WriteString(habbo.Username);
        WriteInteger(chatlogs.Count); // Room Visits Count
        foreach (var chatlog in chatlogs)
        {
            WriteByte(1);
            WriteShort(2); //Count
            WriteString("roomName");
            WriteByte(2);
            WriteString(chatlog.Key.Name); // room name
            WriteString("roomId");
            WriteByte(1);
            WriteInteger(chatlog.Key.Id);
            WriteShort(chatlog.Value.Count); // Chatlogs Count
            foreach (var entry in chatlog.Value)
            {
                var username = "NOT FOUND";
                if (entry.PlayerNullable() != null) username = entry.PlayerNullable().Username;
                WriteString(UnixTimestamp.FromUnixTimestamp(entry.Timestamp).ToShortTimeString());
                WriteInteger(entry.PlayerId); // UserId of message
                WriteString(username); // Username of message
                WriteString(!string.IsNullOrEmpty(entry.Message) ? entry.Message : "** user sent a blank message **"); // Message
                WriteBoolean(habbo.Id == entry.PlayerId);
            }
        }
    }
}