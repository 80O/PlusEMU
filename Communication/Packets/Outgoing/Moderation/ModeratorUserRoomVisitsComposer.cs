using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class ModeratorUserRoomVisitsComposer : ServerPacket
{
    public ModeratorUserRoomVisitsComposer(Habbo data, Dictionary<double, RoomData> visits)
        : base(ServerPacketHeader.ModeratorUserRoomVisitsMessageComposer)
    {
        WriteInteger(data.Id);
        WriteString(data.Username);
        WriteInteger(visits.Count);
        foreach (var visit in visits)
        {
            WriteInteger(visit.Value.Id);
            WriteString(visit.Value.Name);
            WriteInteger(UnixTimestamp.FromUnixTimestamp(visit.Key).Hour);
            WriteInteger(UnixTimestamp.FromUnixTimestamp(visit.Key).Minute);
        }
    }
}