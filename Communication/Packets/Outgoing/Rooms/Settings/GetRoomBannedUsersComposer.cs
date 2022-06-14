using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Settings;

internal class GetRoomBannedUsersComposer : ServerPacket
{
    public GetRoomBannedUsersComposer(Room instance)
        : base(ServerPacketHeader.GetRoomBannedUsersMessageComposer)
    {
        WriteInteger(instance.Id);
        WriteInteger(instance.GetBans().BannedUsers().Count); //Count
        foreach (var id in instance.GetBans().BannedUsers().ToList())
        {
            var data = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(id);
            if (data == null)
            {
                WriteInteger(0);
                WriteString("Unknown Error");
            }
            else
            {
                WriteInteger(data.Id);
                WriteString(data.Username);
            }
        }
    }
}