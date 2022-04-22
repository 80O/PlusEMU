﻿using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

using Plus.Utilities;
using Plus.HabboHotel.Rooms.Chat.Logs;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    class ModeratorRoomChatlogComposer : ServerPacket
    {
        public ModeratorRoomChatlogComposer(Room room, ICollection<ChatlogEntry> chats)
            : base(ServerPacketHeader.ModeratorRoomChatlogMessageComposer)
        {
            WriteByte(1);
            WriteShort(2);//Count
            WriteString("roomName");
            WriteByte(2);
            WriteString(room.Name);
            WriteString("roomId");
            WriteByte(1);
            WriteInteger(room.Id);

            WriteShort(chats.Count);
            foreach (ChatlogEntry entry in chats)
            {
                string username = "Unknown";
                if (entry.PlayerNullable() != null)
                {
                    username = entry.PlayerNullable().Username;
                }

                WriteString(UnixTimestamp.FromUnixTimestamp(entry.Timestamp).ToShortTimeString()); // time?
                WriteInteger(entry.PlayerId); // User Id
                WriteString(username); // Username
                WriteString(!string.IsNullOrEmpty(entry.Message) ? entry.Message : "** user sent a blank message **"); // Message        
                WriteBoolean(false); //TODO, AI's?
            }
        }
    }
}
